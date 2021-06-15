using System;
using RabbitMQ.Client.Events;

namespace XapoWrappers
{
    public interface IXapoQueue
    {
        void Send<T>(T command, string queue);
        void Subscribe(string queueName, EventHandler<BasicDeliverEventArgs> onMessageReceived);
    }
}