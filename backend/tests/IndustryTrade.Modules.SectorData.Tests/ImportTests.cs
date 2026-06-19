using System.Text;
using ClosedXML.Excel;
using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.Import;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Infrastructure.Import;
using Xunit;

namespace IndustryTrade.Modules.SectorData.Tests;

public class ImportParserTests
{
    [Fact]
    public void Excel_parser_reads_header_keyed_rows()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Sheet1");
        ws.Cell(1, 1).Value = "Mã"; ws.Cell(1, 2).Value = "Tên";
        ws.Cell(2, 1).Value = "C1"; ws.Cell(2, 2).Value = "Cụm 1";
        ws.Cell(3, 1).Value = "C2"; ws.Cell(3, 2).Value = "Cụm 2";
        using var ms = new MemoryStream();
        wb.SaveAs(ms); ms.Position = 0;

        var rows = new ExcelFileParser().Parse(ms);

        rows.Should().HaveCount(2);
        rows[0].Cells["Mã"].Should().Be("C1");
        rows[0].Cells["Tên"].Should().Be("Cụm 1");
    }

    [Fact]
    public void Csv_parser_handles_quoted_fields_and_is_case_insensitive_on_lookup()
    {
        var csv = "Mã,Tên\nC1,\"Cụm, có dấu phẩy\"\n";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var rows = new CsvFileParser().Parse(ms);

        rows.Should().ContainSingle();
        rows[0].Cells["Tên"].Should().Be("Cụm, có dấu phẩy");
    }

    [Fact]
    public void Xml_parser_reads_element_per_column()
    {
        var xml = "<rows><row><Ma>C1</Ma><Ten>Cụm 1</Ten></row></rows>";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var rows = new XmlFileParser().Parse(ms);

        rows.Should().ContainSingle();
        rows[0].Cells["Ma"].Should().Be("C1");
    }

    [Fact]
    public void Factory_selects_parser_by_extension()
    {
        var factory = new TabularFileParserFactory(
            [new ExcelFileParser(), new CsvFileParser(), new XmlFileParser()]);

        factory.Resolve("data.xlsx").Should().BeOfType<ExcelFileParser>();
        factory.Resolve("data.csv").Should().BeOfType<CsvFileParser>();
        factory.Resolve("data.xml").Should().BeOfType<XmlFileParser>();
        factory.Resolve("data.txt").Should().BeNull();
    }
}

public class BulkImportHandlerTests
{
    [Fact]
    public async Task Bulk_cluster_import_creates_valid_rows_and_reports_per_row_errors()
    {
        var repo = new FakeClusterRepository();
        var handler = new BulkCreateClustersHandler(repo);
        var unit = Guid.NewGuid();

        var command = new BulkCreateClustersCommand([
            new ClusterImportItem("A", "Valid", unit, null, null, null, ClusterStatus.Operating),
            new ClusterImportItem("B", "", unit, null, null, null, ClusterStatus.Operating),       // blank name → invalid
            new ClusterImportItem("A", "Dup", unit, null, null, null, ClusterStatus.Operating),    // duplicate code
        ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Created.Should().Be(1);
        result.Value.Failed.Should().Be(2);
        result.Value.Errors.Select(e => e.Index).Should().BeEquivalentTo([1, 2]);
        repo.Added.Should().ContainSingle().Which.Code.Should().Be("A");
    }

    private sealed class FakeClusterRepository : IClusterRepository
    {
        public List<IndustrialCluster> Added { get; } = [];

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
            Task.FromResult(Added.Any(c => c.Code == code));
        public Task<IndustrialCluster?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Added.FirstOrDefault(c => c.Id == id));
        public Task AddAsync(IndustrialCluster cluster, CancellationToken ct) { Added.Add(cluster); return Task.CompletedTask; }
        public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => Task.FromResult(true);
        public Task<int> SaveChangesAsync(CancellationToken ct) => Task.FromResult(Added.Count);
        public Task<IReadOnlyList<IndustrialCluster>> ListAsync(Specification<IndustrialCluster> spec, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<IndustrialCluster>>(Added);
        public Task<int> CountAsync(Specification<IndustrialCluster> spec, CancellationToken ct) =>
            Task.FromResult(Added.Count);
    }
}
