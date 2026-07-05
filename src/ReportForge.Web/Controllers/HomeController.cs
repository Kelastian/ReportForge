using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReportForge.Web.Models;

namespace ReportForge.Web.Controllers;

// Kept separate from ReportController so the global exception handler
// (see Program.cs UseExceptionHandler) always has a stable route to fall back to.
public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
