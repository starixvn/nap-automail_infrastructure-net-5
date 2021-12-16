using EasyNetQ;
using EasyNetQ.Topology;
using System;
using System.Threading;

namespace Fanex.MailService.Test
{
    public class TestRabbitMQ
    {
        private const string RabbitMQConnectionString = "host=ha.nexdev.net:5672;virtualHost=/MailStationHost;username=rabbitclient;timeout=60;password=1q2w3e$R";

        public void SendMQ()
        {
            var advancedBus = RabbitHutch.CreateBus(RabbitMQConnectionString).Advanced;
        }

        private void GetMsg(Person person)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FullName: {person.FirstName}.{person.LastName}");
        }
    }
}