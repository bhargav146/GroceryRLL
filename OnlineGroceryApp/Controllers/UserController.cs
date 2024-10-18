using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGroceryApp.Models;
using Serilog;
using System.Text;
using System.Security.Cryptography;
//using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Microsoft.IdentityModel.Tokens;

namespace OnlineGroceryApp.Controllers
{
    public class UserController : Controller
    {


        onlinegroceryDBContext dc = new onlinegroceryDBContext();

        //public readonly IDNTCaptchaValidatorService _validatorService;
        //public UserController(IDNTCaptchaValidatorService validatorService)
        //{
        //    _validatorService = validatorService;
        //}

        public IActionResult Default()
        {
            var res = dc.Users.ToList();
            return View(res);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha", CaptchaGeneratorLanguage = Language.English, CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult Login(string t1, string t2)
        {
            byte[] p = Encoding.UTF8.GetBytes(t2);

            var res = (from t in dc.Users
                       where t.Email == t1 && t.Password == p
                       select t).Count();

            if (res > 0)
            {
               HttpContext.Session.SetString("uid", t1);
                // code to navigate
                return RedirectToAction("home");
            }
            else
            {
                ViewData["err"] = "Invalid username and password.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        //User created successfully

        [HttpPost]
        public IActionResult Register(User u, string password)
        {
            u.Password = Encoding.UTF8.GetBytes(password);
            //    ModelState.MarkFieldValid("Password");
            //   if (ModelState.IsValid)
            {

                dc.Users.Add(u);
                String s = "User created successfully";
                int i = dc.SaveChanges();
                if (i > 0)
                {
                    ViewData["v"] = "User created successfully";
                }

                else
                {
                    ViewData["v"] = "Please correct the errors and try again.";
                }
            }

            return View(u);
        }
        public IActionResult Profile()
        {
            String email = HttpContext.Session.GetString("uid");
            // String email = "saswat@gmail.com";
            var res = from t in dc.Users
                      where t.Email == email
                      select t;
            return View(res.ToList());
        }

        //public IActionResult Cart()
        //{
        //    String email = HttpContext.Session.GetString("uid");
        //    var res = (from t in dc.Users2
        //              where t.Email == email
        //              select t.UserId).FirstOrDefault();
        //    var cartItems = from cart in dc.Cartitems
        //                    where cart.UserId == res
        //                    select new
        //                    {
        //                        CustomerName = cart.User.Name,
        //                        ProductName = cart.Product.ProductName,
        //                        Quantity = cart.Quantity,
        //                        Price = cart.Product.ProductPrice,
        //                        Total = cart.Quantity * cart.Product.ProductPrice
        //                    };

        //    return View(cartItems.ToList());
        //}
        //[HttpPost]
        //public IActionResult BuyNow(int userId, int productId, int quantity)
        //{
        //    return RedirectToAction("Cart", new { userId = userId });
        //}

        public IActionResult OrderHistory()
        {
            String email = HttpContext.Session.GetString("uid");
            var res = (from t in dc.Users
                       where t.Email == email
                       select t.UserId).FirstOrDefault();
            var orders = from order in dc.Orders
                         where order.UserId == res
                         select new
                         {
                             order.OrderId,
                             CustomerName = order.User.Name,
                             ProductName = order.Product.ProductName,
                             order.OrderQty,
                             order.OrderDate,
                             Price = order.Product.ProductPrice,
                             Total = order.OrderQty * order.Product.ProductPrice
                         };

            return View(orders.ToList());
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Home()
        {
            var topProducts = dc.Products.OrderByDescending(p => p.Orders.Sum(o => o.OrderQty)).Take(10).ToList();

            var categories = dc.Categories.ToList();
            ViewBag.Categories = categories;


            return View(topProducts);
        }

        public IActionResult ProductDetails(int id)
        {
            var product = dc.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult BuyNow(int productId, int quantity)
        {
            int userId = GetCurrentUserId();

            var user = dc.Users.Find(userId);
            if (user == null)
            {
                ViewData["error"] = "User does not exist.";
                return View();
            }

            var product = dc.Products.Find(productId);
            if (product == null)
            {
                ViewData["error"] = "Product does not exist.";
                return View();
            }

            var bankAccount = (from t in dc.Fnbcbanks where t.UserId == userId select t).FirstOrDefault();
            if (bankAccount == null)
            {
                bankAccount = new Fnbcbank
                {
                    AccountNumber = new Random().Next(100000, 999999),
                    Balance = 100000,
                    UserId = userId
                };
                dc.Fnbcbanks.Add(bankAccount);
                dc.SaveChanges();
            }

            var model = new
            {
                User = user,
                Product = product,
                Order = new { OrderQty = quantity },
                BankAccount = bankAccount
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateBalance(int accountId, int newBalance)
        {
            var bankAccount = dc.Fnbcbanks.Find(accountId);
            if (bankAccount != null)
            {
                bankAccount.Balance = newBalance;
                dc.SaveChanges();
            }

            return RedirectToAction("BuyNow", new { userId = bankAccount.UserId });
        }

        private int GetCurrentUserId()
        {
            String email = HttpContext.Session.GetString("uid");
            var res = (from t in dc.Users
                       where t.Email == email
                       select t.UserId).FirstOrDefault();
            return res;
        }


        [HttpPost]
        public IActionResult ACart(int id, int quantity)
        {
            var product = dc.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            var user = dc.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var cartItem = dc.Cartitems.FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new Cartitem
                {
                    UserId = userId,
                    ProductId = id,
                    Quantity = quantity
                };
                dc.Cartitems.Add(cartItem);
            }

            dc.SaveChanges();

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            String email = HttpContext.Session.GetString("uid");
            var userId = (from t in dc.Users
                          where t.Email == email
                          select t.UserId).FirstOrDefault();
            var user = dc.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(User user)
        {
            try
            {
                String email = HttpContext.Session.GetString("uid");
                var userId = (from t in dc.Users
                              where t.Email == email
                              select t.UserId).FirstOrDefault();
                var existingUser = dc.Users.FirstOrDefault(u => u.UserId == userId);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.PhoneNo = user.PhoneNo;
                existingUser.Address = user.Address;

                if (user.Password != null && user.Password.Length > 0)
                {
                    existingUser.Password = user.Password;
                }

                dc.Users.Update(existingUser);
                dc.SaveChanges();
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the profile.");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactUs(User user)
        {
            if (ModelState.IsValid)
            {
                // Process the form data (e.g., save to database, send email, etc.)
                ViewBag.Message = "Your message has been sent successfully!";
                return View();
            }

            // If validation fails, return the same view with validation messages
            return View(user);
        }
        //public string d()
        //{
        //    try
        //    {
        //        throw new DivideByZeroException();
        //    }

        //    catch(Exception ex)
        //    {
        //        Log.Error("Error occured: " + ex.Message + " : " + DateTime.Now);
        //    }
        //    return null;

        //}
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]

        public IActionResult ResetPassword(string t, string newPassword, string conPassword, string oldPassword, User u)
        {


            byte[] p = Encoding.UTF8.GetBytes(oldPassword);

            var res = (from r in dc.Users
                       where r.Email == t && r.Password == p
                       select r).FirstOrDefault();

            if (res != null)
            {
                if (newPassword == conPassword)
                {
                    res.Password = Encoding.UTF8.GetBytes(newPassword);

                    int i = dc.SaveChanges();

                    if (i > 0)
                    {
                        ViewData["x"] = "Reset successfully";
                    }
                    else
                    {
                        ViewData["x"] = "Failed to reset password";
                    }
                }
                else
                {
                    ViewData["x"] = "New Password and Confirm Password Mismatch";
                }

            }
            else
            {
                ViewData["x"] = "User not found";
            }

            return View();
        }
        public IActionResult Index()
        {
            // Assuming you have a way to get the current user ID
            int userId = GetCurrentUserId();

            var cartItems = dc.Cartitems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToList();

            return View(cartItems);
        }
        [HttpPost]
        public IActionResult BuyProduct(int productId, int quantity, int accountId)
        {
            var product = dc.Products.Find(productId);
            var bankAccount = dc.Fnbcbanks.Find(accountId);

            if (product == null)
            {
                ViewData["error"] = "Product not found.";
                return View("BuyNow"); // Ensure you have a BuyNow.cshtml view
            }

            if (bankAccount == null)
            {
                // Create a new bank account with a balance of 10,000
                int userId = GetCurrentUserId();
                bankAccount = new Fnbcbank
                {
                    AccountNumber = new Random().Next(100000, 999999),
                    Balance = 10000,
                    UserId = userId
                };
                dc.Fnbcbanks.Add(bankAccount);
                dc.SaveChanges();
            }

            // Calculate total price
            int totalPrice = product.ProductPrice * quantity;

            // Check if the bank account has sufficient balance
            if (bankAccount.Balance < totalPrice)
            {
                ViewData["error"] = "Insufficient balance.";
                return View("BuyNow"); // Ensure you have a BuyNow.cshtml view
            }

            // Deduct the amount from the bank account
            bankAccount.Balance -= totalPrice;

            // Create an order
            var order = new Order
            {
                UserId = bankAccount.UserId,
                ProductId = productId,
                OrderQty = quantity,
                OrderDate = DateTime.Now
            };
            dc.Orders.Add(order);

            // Save changes to the database
            dc.SaveChanges();

            ViewData["message"] = "Purchase successful!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            var order = dc.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = dc.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            var user = dc.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var cartItem = dc.Cartitems.FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new Cartitem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                dc.Cartitems.Add(cartItem);
            }

            dc.SaveChanges();

            return RedirectToAction("Cart");
        }
        [HttpGet]
        public IActionResult Cart()
        {
            int userId = GetCurrentUserId();
            var cartItems = dc.Cartitems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ToList();

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = dc.Cartitems.Find(id);
            if (cartItem != null)
            {
                dc.Cartitems.Remove(cartItem);
                dc.SaveChanges();
            }

            return RedirectToAction("Cart");
        }
        [HttpGet]
        public IActionResult CategoryProducts(int id)
        {
            var products = dc.Products
                .Where(p => p.CategoryId == id)
                .ToList();

            var categoryname = dc.Categories
                        .Where(t => t.CategoryId == id)
                        .Select(t => t.CategoryName)
                        .FirstOrDefault();

            ViewData["name"] = categoryname;

            if (!products.Any())
            {
                ViewData["Message"] = "No products found in this category.";
            }

            return View(products);
        }

        public IActionResult SearchByProductId(int id)
        {
            var res = from t in dc.Products
                      where t.ProductId == id
                      select t;
            return View("SearchResults", res.ToList());
        }

        public IActionResult SearchByProductName(string name)
        {
            var products = dc.Products
        .Where(p => p.ProductName.Contains(name))
        .ToList();

            if (products == null || !products.Any())
            {
                ViewBag.Message = "No products found matching your search.";
                return View("SearchResults", new List<Product>()); // Return an empty list if no products are found
            }

            return View("SearchResults", products);
        }

        public IActionResult SearchByBrandName(string brandName)
        {
            var results = from p in dc.Products
                          join b in dc.Brands on p.BrandId equals b.BrandId
                          where b.BrandName == brandName
                          select new Product
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,
                              ProductPrice = p.ProductPrice,
                              ProductStocks = p.ProductStocks,
                              ProductImage = p.ProductImage,
                              CategoryId = p.CategoryId,
                              ProductDescription = p.ProductDescription
                          };
            return View("SearchResults", results.ToList());
        }

        public IActionResult Search(string query, string searchType)
        {
            if (query == null)
            {
                ViewData["Message"] = "Please enter a search query and select a search type.";
                return View();
            }

            switch (searchType)
            {
                case "ProductId":
                    if (int.TryParse(query, out int productId))
                    {
                        return SearchByProductId(productId);
                    }
                    ViewData["Message"] = "Invalid Product ID.";
                    break;

                case "ProductName":
                    return SearchByProductName(query);

                case "BrandName":
                    return SearchByBrandName(query);

                default:
                    ViewData["Message"] = "Invalid search type.";
                    break;
            }

            return View(new List<Product>());
        }
        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = dc.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                // Logic to cancel the order
                dc.Orders.Remove(order);
                dc.SaveChanges();

                // Optionally, add logic to refund the payment, notify the user, etc.
            }

            // Redirect to a confirmation page or another appropriate action
            return RedirectToAction("Home");
        }


        public ActionResult AddReview(int orderId)
        {
            var review = new Review { OrderId = orderId };
            return View(review);
        }

        [HttpPost]
        public ActionResult AddReview(Review review)
        {
            review.UserId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                dc.Reviews.Add(review);
                dc.SaveChanges();
                return RedirectToAction("OrderHistory");
            }
            return View(review);
        }

        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            return View();
        }


        // [HttpPost]
        //public async Task<IActionResult> SubmitFeedback(Review r)
        //{

        //    string res = null;

        //    using (var httpClient = new HttpClient())
        //    {

        //        using (var response = await httpClient.PostAsJsonAsync("https://localhost:7199/api/Test/fb", r))
        //        {

        //            res = await response.Content.ReadFromJsonAsync<string>();

        //        }


        //    }

        //    ViewData["msg"] = res;
        //    return View();
        //}
        [HttpPost]

        public async Task<ActionResult> SubmitFeedback(Review f)
        {
            // string res = null;

            var httpClient = new HttpClient();


            var response = await httpClient.PostAsJsonAsync("https://localhost:7199/api/Test/fb", f);

            string responseContent = await response.Content.ReadAsStringAsync();




            ViewData["msg"] = "Data Submitted";


            return View();

        }
        [HttpPost]
        public IActionResult BuyAllProducts()
        {
            var userId = GetCurrentUserId();
            var cartItems = dc.Cartitems.Where(c => c.UserId == userId).ToList();

            foreach (var item in cartItems)
            {
               
                var order = new Order
                {
                    UserId = item.UserId,
                    ProductId = item.ProductId,
                    OrderQty = item.Quantity,
                    OrderDate = DateTime.Now
                };
                dc.Orders.Add(order);
            }
            dc.Cartitems.RemoveRange(cartItems);
            dc.SaveChanges();

            return RedirectToAction("Home","User");
        }


    }
}
