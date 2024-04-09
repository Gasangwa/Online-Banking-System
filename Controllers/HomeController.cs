using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ThesisProject.Data;
using ThesisProject.Models;

namespace ThesisProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ThesisProjectContext _context;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public HomeController(ThesisProjectContext context, ILogger<HomeController> logger, IHttpContextAccessor httpcontextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpcontextAccessor = httpcontextAccessor;
        }

        public IActionResult Index()
        {
            if (_httpcontextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/Accounts");
            }
            return View();
        }
        public IActionResult About() 
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Lang(string? Id)
        {
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            string? culture = Id;
            if (culture != null)
            {
                try
                {
                    Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                        );
                }
                catch
                {
                    return Redirect(returnUrl);

                }
            }
            return Redirect(returnUrl);
        }
    }
}
