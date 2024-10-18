using DNTCaptcha.Core.Providers;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using OnlineGroceryApp.Models;
using System.Text;


using System.Security.Cryptography;

namespace OnlineGroceryApp.Controllers
{
    public class RegisterationController : Controller
    {
        public readonly IDNTCaptchaValidatorService _validatorService;
        public RegisterationController(IDNTCaptchaValidatorService validatorService)
        {
            _validatorService = validatorService;
        }

        onlinegroceryDBContext dc = new onlinegroceryDBContext();

        private byte[] HashPassword(string password)
        {
            var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }





        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha",
           CaptchaGeneratorLanguage = Language.English,
           CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]


        public IActionResult Register(Admin r, string password)
        {
            
                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");

                }

                r.Password = Encoding.UTF8.GetBytes(password);
             //   ModelState.MarkFieldValid("Password");
                
                    dc.Admins.Add(r);
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

                


            
            return View(r);


        }




        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]

        public IActionResult ResetPassword(string t, string newPassword, string conPassword, string oldPassword, Admin u)
        {


            byte[] p = Encoding.UTF8.GetBytes(oldPassword);

            var res = (from r in dc.Admins
                       where r.AdminEmail == t && r.Password == p
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
    }
}
