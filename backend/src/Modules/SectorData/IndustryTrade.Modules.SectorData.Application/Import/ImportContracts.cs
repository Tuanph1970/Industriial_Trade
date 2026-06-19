using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Application.Import;

/// <summary>One parsed data row: its source row number and a column-name → cell-value map.</summary>
public sealed record TabularRow(int RowNumber, IReadOnlyDictionary<string, string> Cells);

/// <summary>
/// Reads a tabular upload (Excel / XML / CSV) into header-keyed rows. Implementations are a
/// Strategy set selected by <see cref="ITabularFileParserFactory"/> on the file extension.
/// </summary>
public interface ITabularFileParser
{
    bool CanParse(string fileName);
    IReadOnlyList<TabularRow> Parse(Stream content);
    IReadOnlyList<string> ReadColumns(Stream content);
}

public interface ITabularFileParserFactory
{
    /// <summary>Returns the parser for the file, or null if the extension is unsupported.</summary>
    ITabularFileParser? Resolve(string fileName);
}

// ── Generic parse (preview) ────────────────────────────────────────────────
public sealed record ImportRowDto(int RowNumber, Dictionary<string, string> Cells);

public sealed record ImportParseDto(IReadOnlyList<string> Columns, IReadOnlyList<ImportRowDto> Rows);

/// <summary>
/// Parses an uploaded file into header-keyed rows for client-side preview/validation. Performs no
/// writes and crosses no context boundary — code→id resolution happens on the client, which already
/// holds the indicator/org-unit lists; the typed bulk-create commands do the actual persistence.
/// </summary>
public sealed record ParseImportFileCommand(byte[] Content, string FileName) : ICommand<ImportParseDto>;

public sealed class ParseImportFileHandler(ITabularFileParserFactory factory)
    : ICommandHandler<ParseImportFileCommand, ImportParseDto>
{
    public Task<Result<ImportParseDto>> Handle(ParseImportFileCommand command, CancellationToken ct)
    {
        if (command.Content.Length == 0)
            return Task.FromResult(Result.Failure<ImportParseDto>(Error.Validation("Empty file.")));

        var parser = factory.Resolve(command.FileName);
        if (parser is null)
            return Task.FromResult(Result.Failure<ImportParseDto>(
                Error.Validation("Unsupported file type. Use .xlsx, .xml or .csv.")));

        try
        {
            using var stream = new MemoryStream(command.Content);
            var rows = parser.Parse(stream);
            stream.Position = 0;
            var columns = parser.ReadColumns(stream);
            var dto = new ImportParseDto(
                columns,
                rows.Select(r => new ImportRowDto(r.RowNumber, new Dictionary<string, string>(r.Cells))).ToList());
            return Task.FromResult(Result.Success(dto));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<ImportParseDto>(
                Error.Validation($"Could not read the file: {ex.Message}")));
        }
    }
}

// ── Bulk-create shared result ───────────────────────────────────────────────
public sealed record BulkRowError(int Index, string Message);

public sealed record BulkImportResult(int Created, int Failed, IReadOnlyList<BulkRowError> Errors);
