using Microsoft.AspNetCore.Mvc;
using OnlineGroceryApp.Controllers;
using OnlineGroceryApp.Models;
//using static SkiaSharp.HarfBuzz.SKShaper;

namespace GroceryAppTest
{
    public class UnitTest1
    {
        UserController u = new UserController();
        User us = new User();

        [Fact]
        public void LoginSuccess()
        {
            string email = "saswat@gmail.com";
            string password = "12";
            var result = u.Login(email, password) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("home", result.ActionName);
        }
        [Fact]
        public void LoginFail()
        {
            string email = "saswat@gmail.com";
            string password = "3";
            var result = u.Login(email, password) as RedirectToActionResult;
            Assert.Null(result);



        }
        [Fact]
        public void RegisterSuccess()
        {
            //int id = 100;
            string name = "saswat";
            string email = "555aswa5t3@gmail.com";
            string pass = "3";
            string address = "bbsr";
            string phoneno = "7963362301";

            // us.UserId=id;   
            us.Name = name;
            us.Email = email;
            us.Address = address;
            us.PhoneNo = phoneno;



            var result = u.Register(us, pass) as ViewResult;
            Assert.Equal("User created successfully", result.ViewData["v"]);

        }


    }
}