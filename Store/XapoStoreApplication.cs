using System;
using System.Linq;
using System.Text;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XapoWrappers;

namespace Core
{
    internal class XapoStoreApplication
    {
        private readonly IJsonSerializer _json;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private decimal _btcLevel;
        private readonly IXapoQueue _queue;
        private readonly XapoContext _dbContext;

        public XapoStoreApplication(IJsonSerializer json, IXapoQueue queue, XapoContext dbContext)
        {
            _json = json;
            _queue = queue;
            _dbContext = dbContext;
            _factory = new ConnectionFactory() {HostName = "rabbit"};
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Run()
        {
            _btcLevel = _dbContext.XapoTransaction.Sum(x => x.BtcAmount);
            Subscribe();
        }

        private void Subscribe()
        {
            _channel.QueueDeclare(StoreCommand.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            _channel.BasicConsume(queue: StoreCommand.QueueName,
                autoAck: true,
                consumer: consumer);
            
            Console.ReadLine();
        }

        private void OnMessageReceived(object? _, BasicDeliverEventArgs e)
        {
            Console.WriteLine("store:StoreCommand");
            var json = Encoding.UTF8.GetString(e.Body.ToArray());
            var command = _json.Deserialize<StoreCommand>(json);
            if (command.BtcAmount + _btcLevel <= 100)
            {
                _dbContext.XapoTransaction.Add(new XapoTransaction
                {
                    Action = TransactionAction.Buy,
                    Amount = command.Amount,
                    Charge = command.Charge,
                    Created = command.Created,
                    Currency = command.Currency.ToString(),
                    BtcAmount = command.BtcAmount,
                    PersonId = command.PersonId
                });
                _dbContext.SaveChanges();
                _btcLevel += command.BtcAmount;
            }
            UpdateBtcStatus();
        }

        private void UpdateBtcStatus()
        {
            var updateCurrentBtcLevel = new UpdateCurrentBtcLevel
            {
                CurrentLevel = _btcLevel
            };
            _queue.Send(updateCurrentBtcLevel, UpdateCurrentBtcLevel.QueueName);
        }
    }
}