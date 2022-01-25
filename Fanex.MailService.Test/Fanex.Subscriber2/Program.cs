using EasyNetQ;
using Fanex.Messages;
using System;

namespace Fanex.Subscriber2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus(Common.RabbitMQConnectionString))
            {
                Console.WriteLine("Subscriber2: Listening for messages. Hit <return> to quit.");

                bus.Subscribe<TextMessage>("Test", msg => HandleTextMessage(msg, bus));
                bus.Subscribe<TextMessage2>("Test2", msg => HandleTextMessage2(msg, bus));

                Console.ReadKey();
                bus.Dispose();
            }
        }

        private static void HandleTextMessage(TextMessage textMessage, IBus bus)
        {
            var isconnected = bus.Advanced.IsConnected;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Got message: {0}", textMessage.Text);

            if (textMessage.Text.Equals("Q"))
            {
                Console.WriteLine("Subscriber2: is closed.");
                bus.Dispose();
            }

            Console.ResetColor();
        }

        private static void HandleTextMessage2(TextMessage2 textMessage, IBus bus)
        {
            var isconnected = bus.Advanced.IsConnected;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Got message from Person: {0}", textMessage.Name);
            Console.ResetColor();
        }
    }
}