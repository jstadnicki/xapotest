using System;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    services.AddHttpClient();
                    services.AddTransient<XapoCoreApplication>();
                    services.AddSingleton<IJsonSerializer, NewtonSerializer>();
                    services.AddSingleton<IXapoQueue, RabbitQueueWrapper>();
                    services.AddSingleton<IPriceProvider, CoinDeskPriceProvider>();
                }).UseConsoleLifetime();

            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                services.GetRequiredService<XapoCoreApplication>().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class BuyCommand
    {
        public Guid PersonId { get; set; }
        public CurrencyType Currency { get; set; }
        public decimal Amount { get; set; }
        public static string QueueName => "BuyCommand";
    }

    public class UpdateCurrentBtcLevel
    {
        public decimal CurrentLevel { get; set; }
        public static string QueueName => "UpdateCurrentBtcLevel";
    }
}