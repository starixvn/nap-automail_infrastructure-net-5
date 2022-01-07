using System;

namespace Fanex.Mail.Test
{
    internal class Program
    {
        private const string RabbitMQConnectionString = "host=ha.nexdev.net:5672;virtualHost=/MailStationHost;username=rabbitclient;timeout=60;password=1q2w3e$R";

        private static void Main(string[] args)
        {
            //using (var bus = RabbitHutch.CreateBus(RabbitMQConnectionString))
            //{
            //    var input = string.Empty;
            //    Console.WriteLine("Enter a message. 'Quit' to quit.");
            //    while ((input = Console.ReadLine()) != "Quit")
            //    {
            //        bus.PubSub.Publish(new Person { FirstName = input });
            //        Console.WriteLine("Message published!");
            //    }
            //}
        }
    }
}