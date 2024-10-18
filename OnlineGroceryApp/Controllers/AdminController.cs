using DNTCaptcha.Core.Providers;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using OnlineGroceryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineGroceryApp.Controllers
{
    public class AdminController : Controller
    {
        public readonly IDNTCaptchaValidatorService _validatorService;
        public AdminController(IDNTCaptchaValidatorService validatorService)
        {
            _validatorService = validatorService;
        }

        //   public AdminController() { }

        onlinegroceryDBContext dc = new onlinegroceryDBContext();

        // [Area("Admin")]
        public IActionResult Home()
        {


            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha",
            CaptchaGeneratorLanguage = Language.English,
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]

        public IActionResult Login(string t1, string t2)
        {

           // byte[] p = Encoding.UTF8.GetBytes(t2);


            //if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
            //{
            //    this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
            //}

            //var res = (from t in dc.Admins
            //           where t.AdminName == t1 && t.Password == p
            //           select t).Count();

            //if (res > 0)
            //{
            //    // HttpContext.Session.SetString("uid", t1);
            //    // code to navigate
            //    return RedirectToAction("DashBoard", "products");
            //}
            //else
            //{
            //    ViewData["err"] = "Invalid username and password.";
            //}
            byte[] p = Encoding.UTF8.GetBytes(t2);

            var res = (from t in dc.Admins
                       where t.AdminEmail == t1 && t.Password == p
                       select t).Count();

            if (res > 0)
            {
                HttpContext.Session.SetString("uid1", t1);
                // code to navigate
                return RedirectToAction("DashBoard", "products");
            }
            else
            {
                ViewData["err"] = "Invalid username and password.";
            }



            return View();
        }
        public IActionResult Reports(DateTime? startDate, DateTime? endDate)
        {
            var orders = dc.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value);
            }

            var filteredOrders = orders.Include(o => o.User).Include(o => o.Product).ToList();
            return View(filteredOrders);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login","Admin");
        }

    }

}

    //    if (ModelState.IsValid)
    //    {
    //        if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
    //        {
    //           this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
    //        }

    //      //  byte[] b = Encoding.UTF8.GetBytes(t2);
    //        var res1 = (from t in dc.Admins
    //                    where t.AdminName == t1
    //                    select t).FirstOrDefault();


    //        string st1 = res1.AdminName;
    //        byte[] b1 = res1.AdminPassword;


    //        var res = (from t in dc.Admins
    //                   where t.AdminName == t1 && t.AdminPassword == p 
    //                   select t).Count();

    //        if (res >0)
    //        {
    //           // HttpContext.Session.SetString("uid", t1);

    //           // var claims = new List<Claim>
    //           //{
    //           //  new Claim(ClaimTypes.Name, t1)
    //           // };

    //           // var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    //           // var authProperties = new AuthenticationProperties
    //           // {
    //           //     IsPersistent = true
    //           // };



    //            return RedirectToAction("Home","Admin");
    //        }
    //        else
    //        {
    //            ViewData["r"] = "Invalid User..!!!!";
    //        }
    //    }

    //    return View();
    //}
    //    if (res > 0)
    //    {

    //        HttpContext.Session.SetString("uid", t1);

    //        //code to navigate
    //        return RedirectToAction("Home");

    //    }
    //    else
    //    {
    //        ViewData["r"] = "Invalid User..!!!!";
    //    }
    //}
    //return View();


    
