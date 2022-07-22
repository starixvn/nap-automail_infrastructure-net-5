using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using MailServiceAppSettings = Fanex.MailDeliver.MailClient.AppSettings;

namespace Fanex.MailStation.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            LoadConfiguration();

            return Host.CreateDefaultBuilder(args)
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseUrls($"http://0.0.0.0:5000");
                      webBuilder.UseStartup<Startup>();
                  });
        }

        private static void LoadConfiguration()
        {
            var text = File.ReadAllText("appsettings.json");
            AppConfigService.AppConfig = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
            MailServiceAppSettings.AppMailId = AppConfigService.AppConfig.AppMailId;
            MailServiceAppSettings.AppMailAddress = AppConfigService.AppConfig.AppMailAddress;
        }
    }
}