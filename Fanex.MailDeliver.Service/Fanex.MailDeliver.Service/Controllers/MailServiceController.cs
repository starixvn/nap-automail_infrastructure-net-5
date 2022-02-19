using Fanex.MailDeliver.MailClient.ParamModel;
using Fanex.MailDeliver.Model;
using Fanex.MailDeliver.Service.Service;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fanex.MailDeliver.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailServiceController : ControllerBase
    {
        private readonly IMailService mailService;

        public MailServiceController(IMailService mailService)
        {
            this.mailService = mailService;
        }

        [HttpPost("SendMail")]
        public IActionResult SendMail(MailRecord mailRecord)
        {
            mailService.SendMail(mailRecord);
            return new JsonResult(HttpStatusCode.OK);
        }

        [HttpPost("GetMail")]
        public IActionResult GetMail([FromBody] long mailId)
        {
            var result = mailService.GetMail(mailId);
            return new JsonResult(result);
        }

        [HttpPost("GetListMail")]
        public IActionResult GetListMail(GetListMailParam param)
        {
            var result = mailService.GetListMail(param);
            return new JsonResult(result);
        }

        [HttpPost("GetListMailByDate")]
        public IActionResult GetListMailByDate(GetListMailByDateParam param)
        {
            var result = mailService.GetListMailByDate(param);
            return new JsonResult(result);
        }

        [HttpPost("StoreConfig")]
        public IActionResult StoreConfig(MailConfig param)
        {
            var result = mailService.StoreConfig(param);
            return new JsonResult(result);
        }

        [HttpPost("GetConfig")]
        public IActionResult GetConfig([FromBody] int appId)
        {
            var result = mailService.GetConfig(appId);
            return new JsonResult(result);
        }

        [HttpPost("StoreSetting")]
        public IActionResult StoreSetting(MailSetting mailSetting)
        {
            var result = mailService.StoreSetting(mailSetting);
            return new JsonResult(result);
        }

        [HttpPost("GetSetting")]
        public IActionResult GetSetting([FromBody] int settingId)
        {
            var result = mailService.GetSetting(settingId);
            return new JsonResult(result);
        }

        [HttpPost("ReSendMail")]
        public IActionResult ReSendMail([FromBody] int mailId)
        {
            mailService.ReSendMail(mailId);
            return new JsonResult(HttpContext.Response.StatusCode);
        }

        [HttpPost("StoreFunction")]
        public IActionResult StoreFunction(MailFunction mailFunction)
        {
            var result = mailService.StoreFunction(mailFunction);
            return new JsonResult(result);
        }

        [HttpPost("GetFunction")]
        public IActionResult GetFunction([FromBody] int funcId)
        {
            var result = mailService.GetFunction(funcId);
            return new JsonResult(result);
        }

        [HttpPost("GetListSetting")]
        public IActionResult GetListSetting()
        {
            var result = mailService.GetListSetting();
            return new JsonResult(result);
        }

        [HttpPost("GetListConfig")]
        public IActionResult GetListConfig([FromBody] string serverIp)
        {
            var result = mailService.GetListConfig(serverIp);
            return new JsonResult(result);
        }

        [HttpPost("GetListFuntion")]
        public IActionResult GetListFuntion()
        {
            var result = mailService.GetListFuntion();
            return new JsonResult(result);
        }

        [HttpPost("GetListError")]
        public IActionResult GetListError([FromBody] long mailId)
        {
            var result = mailService.GetListError(mailId);
            return new JsonResult(result);
        }
    }
}