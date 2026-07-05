using System.Globalization;
using ReportForge.Application;
using ReportForge.Domain.Interfaces;
using ReportForge.Infrastructure;

// Fixed regardless of the host OS locale, so dates and currency in the report
// render consistently (e.g. in screenshots) no matter where this is deployed.
var displayCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = displayCulture;
CultureInfo.DefaultThreadCurrentUICulture = displayCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Depend on interfaces here, not concrete types: swapping CsvDataParser or
// ReportService for another implementation never has to touch the Web layer.
builder.Services.AddScoped<IDataParser, CsvDataParser>();
builder.Services.AddScoped<IReportGenerator, ReportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Report}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
