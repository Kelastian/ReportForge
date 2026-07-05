namespace ReportForge.Web.ViewModels;

public sealed class ReportPageViewModel
{
    public string? PasteText { get; init; }
    public string? ErrorMessage { get; init; }
    public ReportViewModel? Report { get; init; }

    public bool HasData => Report is not null;
}
