using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGroceryApp.Models;

namespace OnlineGroceryApp.Controllers
{
    public class ProductController : Controller
    {
        onlinegroceryDBContext dc = new onlinegroceryDBContext();

        private onlinegroceryDBContext CreateContext()
        {
            return new onlinegroceryDBContext(); // Ensure this is correctly instantiated
        }

        // Index - Get the list of products
        [HttpGet]
        public IActionResult Index()
        {
            var products = dc.Products.ToList();

            if (products == null || !products.Any())
            {
                return NotFound("No products found.");
            }

            return View(products);
        }

        // Search Products - Filter products by category or brand
        [HttpGet]
        public IActionResult SearchProducts(string productName)
        {
            var products = dc.Products
                .Where(p => p.ProductName.Contains(productName))
                .ToList();

            if (products == null || !products.Any())
            {
                ViewBag.Message = "No products found matching your search.";
                return View(new List<Product>()); // Return an empty list if no products are found
            }

            return View(products);
        }

        // PostProduct - Create a new product entry
        [HttpGet]
        public IActionResult InsertProduct()
        {
           
            return View();
        }
        [HttpPost]
        public IActionResult InsertProduct(IFormFile file, Product product)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    product.ProductImage = fileName; // Save the file name to the ProductImage field
                }
                else
                {
                    // Provide a default image
                    product.ProductImage = "default.png"; // Make sure to have a default image in the wwwroot/images folder
                }

                dc.Products.Add(product);
                dc.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // EditProduct - Update an existing product
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = dc.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = dc.Categories.ToList();
            ViewBag.Brands = dc.Brands.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product product)
        {
            
                dc.Products.Update(product);
                dc.SaveChanges();
                return RedirectToAction("Index");
            

            ViewBag.Categories = dc.Categories.ToList();
            ViewBag.Brands = dc.Brands.ToList();
            return View(product);
        }


        // ViewProductDescription - View details of a specific product
        [HttpGet]
        public IActionResult ViewProductDescription(int id)
        {
            var product = dc.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // DeleteProduct - Confirm and delete a product
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var product = dc.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = dc.Products.Find(id);
            if (product != null)
            {
                dc.Products.Remove(product);
                dc.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        public IActionResult DashBoard()
        {
           // var orders = from t in dc.Orders
                         //select t;
                         var orders = dc.Orders.Include(m => m.Product).Include(c => c.User).ToList();
            //return View(orders.ToList());
            return View(orders);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var order = dc.Orders.Find(id);
            if (order != null)
            {
                dc.Orders.Remove(order);
                dc.SaveChanges();
            }
            return RedirectToAction("DashBoard");
        }
    }
}
