# Project Context — ReportForge (C# / ASP.NET Core portfolio project)

> This file is the brief for building the project. In Claude Code, keep it at the repo
> root. If you rename it to `CLAUDE.md`, Claude Code loads it automatically every session;
> otherwise start with: "Read context.md and follow it."

---

## 1. Purpose & audience

Portfolio project for **Sebastián Pizarro**, a computer engineer applying for **junior / entry
software roles in Japan** (SES agencies and direct-hire). The goal is to demonstrate solid
**C# / .NET fundamentals, object-oriented and layered design, and clean, readable code** —
NOT feature quantity. Reviewers are technical recruiters and engineers, so the repository must
look intentional and professional. Readability and correctness beat cleverness.

## 2. What we are building

A small **ASP.NET Core MVC** web app called **ReportForge**. The user pastes raw tabular data
(CSV) into a text area and the app generates a clean **business report**: totals, breakdown by
category, top items, and key figures. The user can **paste more data to append** to the current
dataset and regenerate the report. Optional: export the report as CSV.

This "paste data → structured report" theme deliberately mirrors Sebastián's real reporting-SaaS
work and his finance/automation background, so the project reinforces his CV narrative.

## 3. Tech stack

- **.NET 8 (LTS)**, latest C# language version
- **ASP.NET Core MVC** (Razor views)
- **Bootstrap** (bundled with the MVC template) for a clean, screenshot-friendly UI
- **xUnit** for unit tests
- **No database.** Appended data lives in the HTTP **session**. Keep storage behind an interface
  so a real database could be dropped in later without touching business logic.

## 4. Solution architecture (layered / clean separation)

Create a solution `ReportForge.sln` with these projects:

1. **ReportForge.Domain** — pure domain, no framework dependencies.
   - Models: `TransactionRecord` (Date, Category, Description, Amount), `ReportResult`,
     `CategorySummary`. Prefer immutable types (`record` / init-only properties).
   - Interfaces: `IReportGenerator`, `IDataParser`.
2. **ReportForge.Application** — use cases / business logic.
   - `ReportService : IReportGenerator` — computes total, count, average, min/max, date range,
     per-category sums, and top-N categories.
   - Input validation; a custom `DataParsingException` for bad input.
3. **ReportForge.Infrastructure** — implementations of the domain interfaces.
   - `CsvDataParser : IDataParser` — parses pasted CSV into `TransactionRecord`s, tolerant of
     common formatting, with clear error messages on malformed rows.
   - (Optional) a session-backed store for appended data.
4. **ReportForge.Web** — ASP.NET Core MVC (presentation only).
   - `ReportController` actions: `Index` (GET form), `Generate` (POST paste), `Append`
     (POST more data), `ExportCsv`, `Reset`, `LoadSample`.
   - **ViewModels** separate from domain models.
   - DI wiring in `Program.cs` (register `IDataParser` and `IReportGenerator`).
   - Razor views: input textarea, "Generate" + "Paste more" buttons, summary cards + report
     table, a "Load sample data" button, and an export button.
5. **ReportForge.Tests** — xUnit tests for `ReportService` and `CsvDataParser`
   (happy path, malformed input, empty input, and append behavior).

**Dependency direction:** Web → Application → Domain; Infrastructure → Domain; Web references
Infrastructure only to wire DI. **Domain depends on nothing.**

## 5. Design principles to demonstrate

(These are the points a reviewer looks for — make them visible.)

- **SOLID**, especially **dependency inversion**: depend on `IDataParser` / `IReportGenerator`,
  never on concrete classes.
- **Constructor injection** everywhere; no `new`-ing services inside other classes.
- **Immutability** where natural (records / read-only domain models).
- Small, **single-responsibility** classes and methods; guard clauses; explicit error handling.
- **No business logic in controllers or views** — controllers only orchestrate.
- Meaningful names; no dead code.

## 6. Two versions (important — this is a specific request)

Sebastián wants a **teaching version** and a **clean version**:

- Branch **`main`** → the **clean** version. Minimal comments, only where they add real value.
  Production-style.
- Branch **`annotated`** → identical behavior, but with thorough teaching comments. For each
  non-obvious decision add a `// WHY:` comment explaining the reasoning (why an interface here,
  why DI, why this logic lives in Application vs Domain, why immutable, why this test exists).
  This is his study / reference copy.

**Workflow:** finish the clean version on `main` first, then branch `annotated` from it and add
only comments — **do not change behavior** between the two branches.

## 7. README requirements (in English)

- One-paragraph description + 1–2 screenshots of the running app.
- **Architecture** section: short explanation of each layer + a simple dependency diagram
  (ASCII or mermaid).
- **Design decisions** section: 4–6 bullets on the key choices (interfaces/DI, layering,
  immutability, testing). This is the part recruiters actually read.
- **How to run:** `dotnet run --project ReportForge.Web` and `dotnet test`.
- **Tech stack** list.
- (Nice bonus) a short Japanese summary paragraph at the end.

Keep it concise and professional.

## 8. Suggested build order

1. Scaffold the solution + 5 projects; set project references and DI.
2. Domain models + interfaces.
3. `CsvDataParser` + its tests.
4. `ReportService` + its tests.
5. Web: controller, viewmodels, views; wire DI; sample data; append; export.
6. Polish the UI with Bootstrap; capture screenshots.
7. Write the README.
8. Create the `annotated` branch and add `// WHY:` comments.

## 9. Scope guardrails

- Keep it tight — a portfolio piece, not a product. **No auth, no database, no external services.**
- Prefer doing less, but cleaner. If unsure, choose the simpler, more readable option.
- Every public method in Application/Infrastructure gets at least one test, or a clear note on why
  it doesn't need one.
- Commit in small, meaningful steps with clear messages (recruiters may read the commit history).

## 10. Sample data (for the "Load sample" button)

Ship a small built-in CSV so the app demos instantly, e.g.:

```
Date,Category,Description,Amount
2025-01-05,Software,Cloud hosting,120.00
2025-01-08,Office,Coffee supplies,35.50
2025-01-12,Software,Design tool,48.00
2025-02-02,Travel,Client visit,210.75
2025-02-15,Office,Stationery,18.20
2025-02-20,Software,Cloud hosting,120.00
```

Expected report output includes: total amount, record count, average, date range, sum per
category, and the top category by spend.
