using Microsoft.AspNetCore.Mvc;
using OnlineGroceryApp.Models;

namespace OnlineGroceryApp.Controllers
{
    public class ProductsController : Controller
    {
        onlinegroceryDBContext dc = new onlinegroceryDBContext();

        public IActionResult HomePage()
        {
            var topProducts = dc.Products.OrderByDescending(p => p.Orders.Sum(o => o.OrderQty)).Take(10).ToList();

            var categories = dc.Categories.ToList();
            ViewBag.Categories = categories;


            return View(topProducts);
        }



        public IActionResult DashBoard()
        {
            return View();
        }

    }
}
