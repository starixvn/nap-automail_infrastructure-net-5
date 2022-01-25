using EasyNetQ;
using Fanex.Messages;
using System;

namespace Fanex.Publisher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus(Common.RabbitMQConnectionString))
            {
                var input = String.Empty;
                Console.WriteLine("Enter a message. 'Quit' to quit.");
                while ((input = Console.ReadLine()) != "Quit")
                {
                    bus.Publish(new TextMessage { Text = input });
                    Console.WriteLine("Message published!");
                }
            }
        }
    }
}