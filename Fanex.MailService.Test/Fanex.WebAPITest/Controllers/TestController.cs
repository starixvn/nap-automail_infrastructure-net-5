using Fanex.WebAPITest.Model;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.WebAPITest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("SendFullName")]
        public IActionResult SendFullName(Person person)
        {
            person.FullName = person.FirstName + "." + person.LastName;
            return new JsonResult(person);
        }
    }
}