using ReportForge.Application;
using ReportForge.Domain.Exceptions;
using ReportForge.Domain.Models;
using Xunit;

namespace ReportForge.Tests;

public class ReportServiceTests
{
    private readonly ReportService _sut = new();

    private static TransactionRecord Record(string date, string category, decimal amount) => new()
    {
        Date = DateOnly.Parse(date),
        Category = category,
        Description = "n/a",
        Amount = amount
    };

    [Fact]
    public void Generate_WithMixedCategories_ComputesTotalsAndDateRange()
    {
        var records = new[]
        {
            Record("2025-01-05", "Software", 100m),
            Record("2025-02-20", "Office", 50m),
            Record("2025-01-15", "Software", 20m)
        };

        var result = _sut.Generate(records);

        Assert.Equal(3, result.RecordCount);
        Assert.Equal(170m, result.TotalAmount);
        Assert.Equal(20m, result.MinAmount);
        Assert.Equal(100m, result.MaxAmount);
        Assert.Equal(DateOnly.Parse("2025-01-05"), result.StartDate);
        Assert.Equal(DateOnly.Parse("2025-02-20"), result.EndDate);
    }

    [Fact]
    public void Generate_GroupsAndOrdersCategorySummariesByTotalDescending()
    {
        var records = new[]
        {
            Record("2025-01-01", "Office", 10m),
            Record("2025-01-02", "Software", 300m),
            Record("2025-01-03", "Office", 40m)
        };

        var result = _sut.Generate(records);

        Assert.Equal(2, result.CategorySummaries.Count);
        Assert.Equal("Software", result.CategorySummaries[0].Category);
        Assert.Equal(300m, result.CategorySummaries[0].Total);
        Assert.Equal("Office", result.CategorySummaries[1].Category);
        Assert.Equal(50m, result.CategorySummaries[1].Total);
        Assert.Equal(2, result.CategorySummaries[1].Count);
    }

    [Fact]
    public void Generate_WithTopCategoryCount_ReturnsOnlyThatManyTopCategories()
    {
        var records = new[]
        {
            Record("2025-01-01", "A", 10m),
            Record("2025-01-01", "B", 20m),
            Record("2025-01-01", "C", 30m)
        };

        var result = _sut.Generate(records, topCategoryCount: 2);

        Assert.Equal(2, result.TopCategories.Count);
        Assert.Equal("C", result.TopCategories[0].Category);
        Assert.Equal("B", result.TopCategories[1].Category);
    }

    [Fact]
    public void Generate_WithEmptyList_ThrowsDataParsingException()
    {
        Assert.Throws<DataParsingException>(() => _sut.Generate(Array.Empty<TransactionRecord>()));
    }

    [Fact]
    public void Generate_WithZeroTopCategoryCount_ThrowsArgumentOutOfRangeException()
    {
        var records = new[] { Record("2025-01-01", "A", 10m) };

        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Generate(records, topCategoryCount: 0));
    }

    [Fact]
    public void Generate_WithSingleRecord_MinEqualsMaxEqualsAmount()
    {
        var records = new[] { Record("2025-01-01", "A", 42m) };

        var result = _sut.Generate(records);

        Assert.Equal(42m, result.MinAmount);
        Assert.Equal(42m, result.MaxAmount);
        Assert.Equal(42m, result.AverageAmount);
    }

    [Fact]
    public void Generate_OnAppendedBatches_AggregatesCombinedRecordsCorrectly()
    {
        // Simulates the "paste more data" flow: two independently parsed batches
        // are concatenated before a single report is generated over all of it.
        var firstBatch = new[] { Record("2025-01-05", "Software", 100m) };
        var secondBatch = new[] { Record("2025-02-01", "Software", 50m), Record("2025-02-02", "Office", 30m) };

        var result = _sut.Generate(firstBatch.Concat(secondBatch).ToList());

        Assert.Equal(3, result.RecordCount);
        Assert.Equal(180m, result.TotalAmount);
        Assert.Equal("Software", result.CategorySummaries[0].Category);
        Assert.Equal(150m, result.CategorySummaries[0].Total);
    }
}
