using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Fanex.MailStation.Service.Controllers;

using SimpleMailDelivery.Models;
using System.Threading.Tasks;

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

    [HttpGet]
    [Route("Test")]
    public async Task<string> Test()
    {
        var mail = new Mail
        {
            Title = "Test",
            Body = "Test"
        };
        try
        {
            await publishEndpoint.Publish<Mail>(mail);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return "OK";
    }
}