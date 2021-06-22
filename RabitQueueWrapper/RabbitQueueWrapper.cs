using System;
using System.Text;
using Common;
using RabbitMQ.Client;
using XapoWrappers;

namespace RabitQueueWrapper
{
    public class RabbitQueueWrapper : IXapoQueue
    {
        private readonly IJsonSerializer _json;

        public RabbitQueueWrapper(IJsonSerializer json)
        {
            _json = json;
        }

        public void Send<T>(T command, string queue)
        {
            Console.WriteLine("sending...");
            var factory = new ConnectionFactory() { HostName = "rabbit" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string message = _json.Serialize(command);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            Console.WriteLine("sent!");
        }
    }
}