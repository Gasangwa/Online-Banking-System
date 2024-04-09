using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ThesisProject.Data;
using ThesisProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThesisProject.Areas.Identity.Data;
using ThesisProject.Services;
using static ThesisProject.Controllers.AccountsController;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

//Configure DbContext
builder.Services.AddDbContext<ThesisProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BankDbContext") ?? throw new InvalidOperationException("Connection string 'BankDbContext' not found.")));

//configuring user Identity
builder.Services.AddDefaultIdentity<ThesisProjectUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ThesisProjectContext>();
//configuring HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddLocalization(op => op.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR"),
                new CultureInfo("pl-PL")
            };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

var supportedCultures = new[] {"en","fr","pl" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseMiddleware<TotalBalanceMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
