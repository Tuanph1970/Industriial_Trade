using System.Xml.Linq;
using ClosedXML.Excel;
using IndustryTrade.Modules.SectorData.Application.Import;

namespace IndustryTrade.Modules.SectorData.Infrastructure.Import;

/// <summary>Selects a parser by file extension (the Strategy switch).</summary>
internal sealed class TabularFileParserFactory(IEnumerable<ITabularFileParser> parsers) : ITabularFileParserFactory
{
    public ITabularFileParser? Resolve(string fileName) =>
        parsers.FirstOrDefault(p => p.CanParse(fileName));
}

/// <summary>Parses .xlsx via ClosedXML. The first non-empty row is the header.</summary>
internal sealed class ExcelFileParser : ITabularFileParser
{
    public bool CanParse(string fileName) => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> ReadColumns(Stream content)
    {
        using var wb = new XLWorkbook(content);
        var ws = wb.Worksheets.First();
        var headerRow = ws.FirstRowUsed();
        return headerRow is null ? [] : ReadHeaders(headerRow);
    }

    public IReadOnlyList<TabularRow> Parse(Stream content)
    {
        using var wb = new XLWorkbook(content);
        var ws = wb.Worksheets.First();
        var headerRow = ws.FirstRowUsed();
        if (headerRow is null) return [];

        var headers = ReadHeaders(headerRow);
        var rows = new List<TabularRow>();
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                var value = row.Cell(i + 1).GetString().Trim();
                if (!string.IsNullOrEmpty(value)) cells[headers[i]] = value;
            }
            if (cells.Count > 0) rows.Add(new TabularRow(row.RowNumber(), cells));
        }
        return rows;
    }

    private static List<string> ReadHeaders(IXLRow headerRow)
    {
        var headers = new List<string>();
        foreach (var cell in headerRow.CellsUsed())
            headers.Add(cell.GetString().Trim());
        return headers;
    }
}

/// <summary>Parses CSV (comma-separated, optional double-quoted fields). First line is the header.</summary>
internal sealed class CsvFileParser : ITabularFileParser
{
    public bool CanParse(string fileName) => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> ReadColumns(Stream content)
    {
        using var reader = new StreamReader(content);
        var header = reader.ReadLine();
        return header is null ? [] : SplitCsv(header);
    }

    public IReadOnlyList<TabularRow> Parse(Stream content)
    {
        using var reader = new StreamReader(content);
        var header = reader.ReadLine();
        if (header is null) return [];

        var headers = SplitCsv(header);
        var rows = new List<TabularRow>();
        var lineNumber = 1;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = SplitCsv(line);
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count && i < values.Count; i++)
            {
                var value = values[i].Trim();
                if (!string.IsNullOrEmpty(value)) cells[headers[i]] = value;
            }
            if (cells.Count > 0) rows.Add(new TabularRow(lineNumber, cells));
        }
        return rows;
    }

    private static List<string> SplitCsv(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { fields.Add(current.ToString().Trim()); current.Clear(); }
            else current.Append(c);
        }
        fields.Add(current.ToString().Trim());
        return fields;
    }
}

/// <summary>
/// Parses XML shaped as &lt;rows&gt;&lt;row&gt;&lt;ColumnName&gt;value&lt;/ColumnName&gt;…&lt;/row&gt;&lt;/rows&gt;.
/// Column names are the union of child-element names across rows (first-seen order).
/// </summary>
internal sealed class XmlFileParser : ITabularFileParser
{
    public bool CanParse(string fileName) => fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> ReadColumns(Stream content)
    {
        var doc = XDocument.Load(content);
        var columns = new List<string>();
        foreach (var row in RowElements(doc))
            foreach (var field in row.Elements())
                if (!columns.Contains(field.Name.LocalName)) columns.Add(field.Name.LocalName);
        return columns;
    }

    public IReadOnlyList<TabularRow> Parse(Stream content)
    {
        var doc = XDocument.Load(content);
        var rows = new List<TabularRow>();
        var rowNumber = 1;
        foreach (var rowEl in RowElements(doc))
        {
            rowNumber++;
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in rowEl.Elements())
            {
                var value = field.Value.Trim();
                if (!string.IsNullOrEmpty(value)) cells[field.Name.LocalName] = value;
            }
            if (cells.Count > 0) rows.Add(new TabularRow(rowNumber, cells));
        }
        return rows;
    }

    private static IEnumerable<XElement> RowElements(XDocument doc) =>
        doc.Root?.Elements() ?? [];
}
