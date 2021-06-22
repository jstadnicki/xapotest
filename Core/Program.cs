using System;
using System.Threading.Tasks;
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
                Console.WriteLine("Await 10s for rabbit...");
                Task.Delay(25*1000).GetAwaiter().GetResult();
                Console.WriteLine("core core core");
                services.GetRequiredService<XapoCoreApplication>().RunAsync();
                Console.WriteLine("core:update btc");

                host.Run();
                Console.WriteLine("core is dead");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}