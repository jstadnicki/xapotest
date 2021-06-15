using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabitQueueWrapper;
using XapoWrappers;

namespace Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<XapoStoreApplication>();
                    services.AddSingleton<IJsonSerializer, NewtonSerializer>();
                    services.AddSingleton<IXapoQueue, RabbitQueueWrapper>();
                }).UseConsoleLifetime();

            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                services.GetRequiredService<XapoStoreApplication>().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured");
            }
        }
    }

    internal class XapoStoreApplication
    {
        private readonly IJsonSerializer _json;
        private readonly List<StoreCommand> _store = new List<StoreCommand>();
        private readonly ConnectionFactory _factory;

        public XapoStoreApplication(IJsonSerializer json)
        {
            _json = json;
            _factory = new ConnectionFactory() {HostName = "localhost"};
        }

        public void Run()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(StoreCommand.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnMessageReceived;
            channel.BasicConsume(queue: StoreCommand.QueueName,
                autoAck: true,
                consumer: consumer);

            Console.ReadLine();
        }

        private void OnMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            var json = Encoding.UTF8.GetString(e.Body.ToArray());
            var command = _json.Deserialize<StoreCommand>(json);
            _store.Add(command);
        }
    }

    public class StoreCommand
    {
        public DateTime Created { get; set; }
        public decimal BtcAmount { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid PersonId { get; set; }
        public static string QueueName => "StoreCommand";
        public decimal Charge { get; set; }
    }
}