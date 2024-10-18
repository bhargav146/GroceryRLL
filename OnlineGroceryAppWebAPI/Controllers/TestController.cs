using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineGroceryAppWebAPI.Models;

namespace OnlineGroceryAppWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        onlinegroceryyDBContext db = new onlinegroceryyDBContext();

        [HttpPost]
        [Route("fb")]

        public IActionResult Feedback(Review r)
        {

            db.Reviews.Add(r);
            db.SaveChanges();

            return Ok("Thanks for your Review!!!");

        }

    }
}
