using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.MailStation.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller(IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    [Route("")]
    public ContentResult Index()
    {
        var text = System.IO.File.ReadAllText("wwwroot/default.htm");
        return new ContentResult
        {
            ContentType = "text/html",
            Content = text
        };
    }
}