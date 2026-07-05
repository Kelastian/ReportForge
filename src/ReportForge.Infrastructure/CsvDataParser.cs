using System.Globalization;
using System.Text;
using ReportForge.Domain.Exceptions;
using ReportForge.Domain.Interfaces;
using ReportForge.Domain.Models;

namespace ReportForge.Infrastructure;

public sealed class CsvDataParser : IDataParser
{
    public IReadOnlyList<TransactionRecord> Parse(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            throw new DataParsingException(
                "No data provided. Paste CSV rows with Date, Category, Description, and Amount columns.");
        }

        var lines = rawData.Split('\n');
        var records = new List<TransactionRecord>();
        var isFirstContentLine = true;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim('\r', ' ', '\t');
            if (line.Length == 0)
            {
                continue;
            }

            if (isFirstContentLine)
            {
                isFirstContentLine = false;
                if (IsHeaderRow(line))
                {
                    continue;
                }
            }

            records.Add(ParseLine(line, i + 1));
        }

        if (records.Count == 0)
        {
            throw new DataParsingException("No valid data rows were found.");
        }

        return records;
    }

    private static bool IsHeaderRow(string line)
    {
        var fields = SplitCsvLine(line);
        return fields.Count > 0 && fields[0].Trim().Equals("Date", StringComparison.OrdinalIgnoreCase);
    }

    private static TransactionRecord ParseLine(string line, int lineNumber)
    {
        var fields = SplitCsvLine(line);
        if (fields.Count != 4)
        {
            throw new DataParsingException(
                $"Line {lineNumber}: expected 4 columns (Date, Category, Description, Amount) but found {fields.Count}.");
        }

        var rawDate = fields[0].Trim();
        if (!DateOnly.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new DataParsingException($"Line {lineNumber}: '{rawDate}' is not a valid date.");
        }

        var category = fields[1].Trim();
        if (category.Length == 0)
        {
            throw new DataParsingException($"Line {lineNumber}: Category cannot be empty.");
        }

        var description = fields[2].Trim();

        var rawAmount = fields[3].Trim();
        if (!decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            throw new DataParsingException($"Line {lineNumber}: '{rawAmount}' is not a valid amount.");
        }

        return new TransactionRecord
        {
            Date = date,
            Category = category,
            Description = description,
            Amount = amount
        };
    }

    // A naive Split(',') would break if Description itself contains a comma, so
    // quoted fields are honored here (and "" inside quotes is an escaped quote).
    private static IReadOnlyList<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
