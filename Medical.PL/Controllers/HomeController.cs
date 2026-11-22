using System.Diagnostics;
using Medical.BLL.ViewModels.Home;
using Medical.PL.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Medical.PL.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["LayoutMode"] = "FullPage";
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
    }
}
