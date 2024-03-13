using EasyNetQ;
using Fanex.MailDeliver.MailClient.Model;
using Fanex.Messages;
using System;

namespace Fanex.Subscriber
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus(Common.RabbitMQConnectionString))
            {
                Console.WriteLine("Listening for messages. Hit <return> to quit.");

                bus.PubSub.Subscribe<TextMessage>("Test", msg => HandleTextMessage(msg, bus));
                bus.PubSub.Subscribe<TextMessage2>("Test2", msg => HandleTextMessage2(msg, bus));
                bus.PubSub.Subscribe<MailRecord>("MailRecord", mail => HandleMailRecord(mail, bus));
                Console.WriteLine("Start Hitting ... ");
                Console.ReadKey();
                bus.Dispose();
            }
        }

        private static void HandleTextMessage(TextMessage textMessage, IBus bus)
        {
            var isconnected = bus.Advanced.IsConnected;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", textMessage.Text);
            Console.ResetColor();
        }

        private static void HandleTextMessage2(TextMessage2 textMessage, IBus bus)
        {
            var isconnected = bus.Advanced.IsConnected;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Got message from Person: {0}", textMessage.Name);
            Console.ResetColor();
        }

        private static void HandleMailRecord(MailRecord mailRecord, IBus bus)
        {
            var isconnected = bus.Advanced.IsConnected;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Got message from MailRecord: {0}", mailRecord.MailTo + "-" + mailRecord.Subject);
            Console.ResetColor();
        }
    }
}