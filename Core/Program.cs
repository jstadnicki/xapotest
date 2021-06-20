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
}