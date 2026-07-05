using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ReportForge.Domain.Exceptions;
using ReportForge.Domain.Interfaces;
using ReportForge.Domain.Models;
using ReportForge.Web.ViewModels;

namespace ReportForge.Web.Controllers;

public class ReportController : Controller
{
    private const string SessionKey = "ReportForge.Transactions";

    private readonly IDataParser _dataParser;
    private readonly IReportGenerator _reportGenerator;

    public ReportController(IDataParser dataParser, IReportGenerator reportGenerator)
    {
        _dataParser = dataParser;
        _reportGenerator = reportGenerator;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var records = GetStoredRecords();

        var viewModel = new ReportPageViewModel
        {
            PasteText = TempData["PasteText"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
            Report = records.Count > 0 ? BuildReportViewModel(records) : null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Generate(string? pasteText)
    {
        try
        {
            SaveRecords(_dataParser.Parse(pasteText ?? string.Empty));
        }
        catch (DataParsingException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["PasteText"] = pasteText;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Append(string? pasteText)
    {
        try
        {
            var newRecords = _dataParser.Parse(pasteText ?? string.Empty);
            SaveRecords(GetStoredRecords().Concat(newRecords).ToList());
        }
        catch (DataParsingException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["PasteText"] = pasteText;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LoadSample()
    {
        SaveRecords(_dataParser.Parse(SampleData.Csv));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ExportCsv()
    {
        var records = GetStoredRecords();
        if (records.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var csv = new StringBuilder("Date,Category,Description,Amount\n");
        foreach (var record in records)
        {
            csv.Append(record.Date.ToString("yyyy-MM-dd"))
                .Append(',').Append(EscapeCsvField(record.Category))
                .Append(',').Append(EscapeCsvField(record.Description))
                .Append(',').Append(record.Amount)
                .Append('\n');
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "reportforge-data.csv");
    }

    private ReportViewModel BuildReportViewModel(IReadOnlyList<TransactionRecord> records)
    {
        var report = _reportGenerator.Generate(records);

        CategorySummaryViewModel ToViewModel(CategorySummary summary) => new()
        {
            Category = summary.Category,
            Total = summary.Total,
            Count = summary.Count,
            PercentageOfTotal = report.TotalAmount == 0
                ? 0
                : Math.Round(summary.Total / report.TotalAmount * 100, 1)
        };

        return new ReportViewModel
        {
            RecordCount = report.RecordCount,
            TotalAmount = report.TotalAmount,
            AverageAmount = report.AverageAmount,
            MinAmount = report.MinAmount,
            MaxAmount = report.MaxAmount,
            StartDate = report.StartDate,
            EndDate = report.EndDate,
            CategorySummaries = report.CategorySummaries.Select(ToViewModel).ToList(),
            TopCategories = report.TopCategories.Select(ToViewModel).ToList()
        };
    }

    private IReadOnlyList<TransactionRecord> GetStoredRecords()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        if (json is null)
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<TransactionRecord>>(json) ?? [];
    }

    private void SaveRecords(IReadOnlyList<TransactionRecord> records)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(records));
    }

    private static string EscapeCsvField(string field) =>
        field.Contains(',') || field.Contains('"')
            ? $"\"{field.Replace("\"", "\"\"")}\""
            : field;
}
