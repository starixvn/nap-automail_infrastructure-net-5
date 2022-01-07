using Microsoft.AspNetCore.Mvc;

namespace Fanex.MailDeliver.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return new OkResult();
        }
    }
}