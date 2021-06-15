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
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnMessageReceived;
            channel.BasicConsume(queue: BuyCommand.QueueName,
                autoAck: true,
                consumer: consumer);

            Console.ReadLine();
        }

        private void OnMessageReceived(object? sender, BasicDeliverEventArgs e)
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
            var btcToBuy = Math.Round(command.Amount / currentRate, 8);

            if (btcToBuy + _currentBtc < BtcLimit)
            {
                var actualCharge = btcToBuy * currentRate;
                StoreBtcAsync(command, btcToBuy, actualCharge);
                _currentBtc += btcToBuy;
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