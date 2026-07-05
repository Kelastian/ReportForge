using ReportForge.Domain.Exceptions;
using ReportForge.Infrastructure;
using Xunit;

namespace ReportForge.Tests;

public class CsvDataParserTests
{
    private readonly CsvDataParser _sut = new();

    [Fact]
    public void Parse_WithHeaderRow_SkipsHeaderAndReturnsRecords()
    {
        var csv = "Date,Category,Description,Amount\n2025-01-05,Software,Cloud hosting,120.00\n2025-01-08,Office,Coffee supplies,35.50";

        var records = _sut.Parse(csv);

        Assert.Equal(2, records.Count);
        Assert.Equal("Software", records[0].Category);
        Assert.Equal(120.00m, records[0].Amount);
    }

    [Fact]
    public void Parse_WithoutHeaderRow_ReturnsAllRowsAsData()
    {
        var csv = "2025-01-05,Software,Cloud hosting,120.00";

        var records = _sut.Parse(csv);

        Assert.Single(records);
        Assert.Equal(DateOnly.Parse("2025-01-05"), records[0].Date);
    }

    [Fact]
    public void Parse_IgnoresBlankLinesAndTrailingWhitespace()
    {
        var csv = "Date,Category,Description,Amount\n\n  2025-01-05,Software,Cloud hosting,120.00  \r\n\n";

        var records = _sut.Parse(csv);

        Assert.Single(records);
    }

    [Fact]
    public void Parse_WithQuotedDescriptionContainingComma_KeepsCommaInsideField()
    {
        var csv = "2025-01-05,Travel,\"Client visit, dinner included\",210.75";

        var records = _sut.Parse(csv);

        Assert.Equal("Client visit, dinner included", records[0].Description);
    }

    [Fact]
    public void Parse_NullOrWhitespaceInput_ThrowsDataParsingException()
    {
        Assert.Throws<DataParsingException>(() => _sut.Parse(""));
        Assert.Throws<DataParsingException>(() => _sut.Parse("   "));
    }

    [Fact]
    public void Parse_RowWithWrongColumnCount_ThrowsDataParsingExceptionWithLineNumber()
    {
        var csv = "Date,Category,Description,Amount\n2025-01-05,Software,120.00";

        var ex = Assert.Throws<DataParsingException>(() => _sut.Parse(csv));

        Assert.Contains("Line 2", ex.Message);
    }

    [Fact]
    public void Parse_RowWithInvalidDate_ThrowsDataParsingException()
    {
        var csv = "not-a-date,Software,Cloud hosting,120.00";

        Assert.Throws<DataParsingException>(() => _sut.Parse(csv));
    }

    [Fact]
    public void Parse_RowWithInvalidAmount_ThrowsDataParsingException()
    {
        var csv = "2025-01-05,Software,Cloud hosting,not-a-number";

        Assert.Throws<DataParsingException>(() => _sut.Parse(csv));
    }

    [Fact]
    public void Parse_RowWithEmptyCategory_ThrowsDataParsingException()
    {
        var csv = "2025-01-05,,Cloud hosting,120.00";

        Assert.Throws<DataParsingException>(() => _sut.Parse(csv));
    }

    [Fact]
    public void Parse_CalledTwice_IsStatelessAcrossCalls()
    {
        // The parser must not leak state between calls, since the "paste more data"
        // flow parses each new batch with the same parser instance.
        var first = _sut.Parse("2025-01-05,Software,Cloud hosting,120.00");
        var second = _sut.Parse("2025-02-01,Office,Stationery,18.20");

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal("Software", first[0].Category);
        Assert.Equal("Office", second[0].Category);
    }
}
