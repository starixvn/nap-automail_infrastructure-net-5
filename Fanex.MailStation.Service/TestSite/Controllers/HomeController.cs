using Microsoft.AspNetCore.Mvc;

namespace TestSite.Controllers
{
    using Fanex.SimpleMailDelivery;
    using Fanex.SimpleMailDelivery.Models;

    [ApiController]
    [Route("")]
    public class HomeController(MailClient mailClient) : ControllerBase
    {
        [Route("")]
        [HttpGet]
        public async Task<string> Get()
        {
            var mail = new Mail
            {
                Title = "Test",
                Body = "Test",
                MailingListId = 53
            };
            await mailClient.SendFunctionMailAsync(mail);
            return "OK";
        }
    }
}
