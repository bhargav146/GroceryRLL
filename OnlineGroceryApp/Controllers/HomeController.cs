using Microsoft.AspNetCore.Mvc;
using OnlineGroceryApp.Models;
using System.Diagnostics;

namespace OnlineGroceryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        onlinegroceryDBContext dc = new onlinegroceryDBContext();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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
            return View();
        }
        public IActionResult Default()
        {
            var res = dc.Users.ToList();
            return View(res);
        }
    }
}
