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
            _factory = new ConnectionFactory() {HostName = "localhost", Password = "guest", UserName = "guest"};
        }

        public void RunAsync()
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(BuyCommand.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var buyCommandConsumer = new EventingBasicConsumer(channel);
            buyCommandConsumer.Received += OnBuyCommandReceived;
            channel.BasicConsume(queue: BuyCommand.QueueName,
                autoAck: true,
                consumer: buyCommandConsumer);
            
            channel.QueueDeclare(UpdateCurrentBtcLevel.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var updateCurrentBtcLevelConsumer = new EventingBasicConsumer(channel);
            updateCurrentBtcLevelConsumer.Received += OnEventingBasicConsumerReceived;
            channel.BasicConsume(queue: UpdateCurrentBtcLevel.QueueName,
                autoAck: true,
                consumer: updateCurrentBtcLevelConsumer);

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