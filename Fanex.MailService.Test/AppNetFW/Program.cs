using Fanex.MailDeliver.MailClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetFW
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppSettings.AppMailId = "1";
            AppSettings.AppMailAddress = "http://localhost:31599/";
        }
    }
}