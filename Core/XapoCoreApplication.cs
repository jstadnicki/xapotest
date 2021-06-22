using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XapoWrappers;

namespace Core
{
    public class XapoCoreApplication
    {
        private IJsonSerializer _json;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IXapoQueue _queue;
        private readonly IPriceProvider _priceProvider;
        private decimal _currentBtc;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public XapoCoreApplication(
            IJsonSerializer json,
            IHttpClientFactory httpClientFactory,
            IXapoQueue queue,
            IPriceProvider priceProvider)
        {
            _json = json;
            _httpClientFactory = httpClientFactory;
            _queue = queue;
            _priceProvider = priceProvider;
            _factory = new ConnectionFactory() {HostName = "rabbit"};
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void RunAsync()
        {
            _channel.BasicQos(0, 1, false);

            Console.WriteLine("core-runasync");
            Console.WriteLine("core:declaring queues");
            _channel.QueueDeclare(BuyCommand.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var buyCommandConsumer = new EventingBasicConsumer(_channel);
            buyCommandConsumer.Received += OnBuyCommandReceived;
            Console.WriteLine("core:buy command");

            _channel.BasicConsume(queue: BuyCommand.QueueName,
                autoAck: true,
                consumer: buyCommandConsumer);

            _channel.QueueDeclare(UpdateCurrentBtcLevel.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var updateCurrentBtcLevelConsumer = new EventingBasicConsumer(_channel);
            updateCurrentBtcLevelConsumer.Received += OnEventingBasicConsumerReceived;
            _channel.BasicConsume(queue: UpdateCurrentBtcLevel.QueueName,
                autoAck: true,
                consumer: updateCurrentBtcLevelConsumer);
            Console.WriteLine("core:update btc");
            Console.ReadLine();
        }

        private void OnEventingBasicConsumerReceived(object? sender, BasicDeliverEventArgs e)
        {
            var json = Encoding.UTF8.GetString(e.Body.ToArray());
            var command = _json.Deserialize<UpdateCurrentBtcLevel>(json);
            _currentBtc = command.CurrentLevel;
        }

        private void OnBuyCommandReceived(object? sender, BasicDeliverEventArgs e)
        {
            Console.WriteLine("Core:BuyCommand");
            var json = Encoding.UTF8.GetString(e.Body.ToArray());
            var command = _json.Deserialize<BuyCommand>(json);
            HandleCommand(command);
        }

        private void HandleCommand(BuyCommand command)
        {
            var price = _priceProvider.GetLatestAsync(_httpClientFactory.CreateClient()).GetAwaiter().GetResult();
            var currency = price.Bpi[command.Currency.ToString()];

            var currentRate = decimal.Parse(currency.Rate, CultureInfo.InvariantCulture);
            var btcToBuy = Math.Round(command.Amount / currentRate, 8, MidpointRounding.ToZero);

            if (btcToBuy + _currentBtc < BtcLimit)
            {
                Console.WriteLine("Core:->StoreBtcAsync");
                var actualCharge = btcToBuy * currentRate;
                StoreBtcAsync(command, btcToBuy, actualCharge);
            }
            //else
            // ? notify person about limit reach
            // ? store into another queue for future use?
            // ? how to handle cash freeze  
        }

        private void StoreBtcAsync(BuyCommand command, decimal btcToBuy, decimal actualCharge)
        {
            var store = new StoreCommand
            {
                Created = DateTime.UtcNow,
                BtcAmount = btcToBuy,
                Amount = command.Amount,
                Currency = command.Currency,
                PersonId = command.PersonId,
                Charge = actualCharge
            };

            _queue.Send(store, StoreCommand.QueueName);
        }

        public decimal BtcLimit => 100;
    }
}