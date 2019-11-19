using System;
using System.Text.Json;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure;
using Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure.EventHandlers;
using Liyanjie.FakeMQ.Sample.Console.NetCore.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore
{
    class Program
    {
        static void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(options =>
            {
                options.Serialize = @object => JsonSerializer.Serialize(@object);
                options.Deserialize = (@string, type) => JsonSerializer.Deserialize(@string, type);
                options.GetEventStore = serviceProvider => serviceProvider.GetRequiredService<IFakeMQEventStore>();
                options.GetProcessStore = serviceProvider => serviceProvider.GetRequiredService<IFakeMQProcessStore>();
            });
        }
        static void InitializeDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<DataContext>();
            context.Database.EnsureCreated();
        }
        static async Task ConfigureEventBusAsync(IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetRequiredService<FakeMQEventBus>();
            await eventBus.SubscribeAsync<MessageEvent, MessageEventHandler>();
        }

        static async Task<bool> ShowMessagesAsync(IServiceProvider serviceProvider)
        {
            System.Console.WriteLine("################################");
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<DataContext>();
            foreach (var item in await context.Messages.ToListAsync())
            {
                System.Console.WriteLine($"{item.Id}=>{item.Content}");
            }
            System.Console.WriteLine("################################");
            System.Console.WriteLine();

            return true;
        }

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            InitializeDatabase(serviceProvider);

            await ConfigureEventBusAsync(serviceProvider);

            await FakeMQ.StartAsync();

            while (true)
            {
                System.Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                switch (input)
                {
                    case "0":
                        await ShowMessagesAsync(serviceProvider);
                        break;
                    case "1":
                        await FakeMQ.EventBus.PublishAsync(new MessageEvent { Message = $"Action1:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" });
                        break;
                    case "2":
                        await FakeMQ.EventBus.PublishAsync(new MessageEvent { Message = $"Action2:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" });
                        break;
                    case "00":
                        return;
                    default:
                        break;
                };
            }
        }
    }
}
