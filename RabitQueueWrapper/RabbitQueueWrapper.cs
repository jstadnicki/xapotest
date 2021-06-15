﻿using System;
using System.Text;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message = _json.Serialize(command);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            }
        }

        public void Subscribe(string queueName, EventHandler<BasicDeliverEventArgs> onMessageReceived)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queueName, 
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += onMessageReceived;
        }
    }
}