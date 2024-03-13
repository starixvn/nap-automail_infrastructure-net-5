using EasyNetQ;
using Fanex.MailDeliver.MailClient.Model;
using Fanex.Messages;
using System;

namespace Fanex.Publisher.Person
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus(Common.RabbitMQConnectionString))
            {
                var input = String.Empty;
                Console.WriteLine("Enter a message for Person. 'Quit' to quit.");
                while ((input = Console.ReadLine()) != "Quit")
                {
                    bus.PubSub.Publish(new TextMessage2 { Name = input });
                    bus.PubSub.Publish(new MailRecord { MailTo = "khoatran094@gmail.com", Subject = "Subject-" + DateTime.Now });
                    Console.WriteLine("Message published!");
                }
            }
        }
    }
}