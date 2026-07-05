using ReportForge.Domain.Models;

namespace ReportForge.Domain.Interfaces;

public interface IDataParser
{
    IReadOnlyList<TransactionRecord> Parse(string rawData);
}
