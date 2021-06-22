using System;
using System.IO;
using System.Threading.Tasks;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var builder =
                Host.CreateDefaultBuilder()
                    .ConfigureHostConfiguration(d => { d.AddJsonFile("appsettings.json"); })
                    .ConfigureServices((hostContext, services) =>
                    {
                        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                        services.AddTransient<XapoStoreApplication>();
                        services.AddSingleton<IJsonSerializer, NewtonSerializer>();
                        services.AddSingleton<IXapoQueue, RabbitQueueWrapper>();
                        services.AddDbContext<XapoContext>(o => o.UseSqlServer("name=DefaultConnection"), ServiceLifetime.Singleton);
                        services.AddSingleton<IConfiguration>(configuration);
                    }).UseConsoleLifetime();

            var host = builder.Build();
            Console.WriteLine("Await 10s for rabbit...");
            Task.Delay(25*1000).GetAwaiter().GetResult();
            Console.WriteLine("Store Store Store");
            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;
            try
            {
                var xapoContext = services.GetService<XapoContext>();
                xapoContext.Database.Migrate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            try
            {
                Console.WriteLine("store running");
                services.GetRequiredService<XapoStoreApplication>().Run();
                host.Run();
                Console.WriteLine("store closing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Occured: {ex}");
            }
        }
    }
}