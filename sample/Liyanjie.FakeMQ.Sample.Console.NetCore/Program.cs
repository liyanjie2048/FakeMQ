using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
            services.AddDbContext<DataContext>();

            services.AddTransient<IFakeMQEventStore, FakeMQEventStore>();
            services.AddTransient<IFakeMQProcessStore, FakeMQProcessStore>();
            services.AddSingleton(serviceProvider => new FakeMQEventBus(serviceProvider.CreateScope().ServiceProvider));

            FakeMQ.Serialize = @object => JsonSerializer.Serialize(@object);
            FakeMQ.Deserialize = (@string, type) => JsonSerializer.Deserialize(@string, type);
        }
        static void InitializeDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<DataContext>();
            context.Database.EnsureCreated();
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

            FakeMQ.Initialize(serviceProvider.GetService<FakeMQEventBus>());
            await FakeMQ.EventBus.SubscribeAsync<MessageEvent, MessageEventHandler>();
            await FakeMQ.StartAsync();

            while (true)
            {
                System.Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                var result = input switch
                {
                    "0" => await ShowMessagesAsync(serviceProvider),
                    "1" => await FakeMQ.EventBus.PublishAsync(new MessageEvent { Message = $"Action1:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" }),
                    "2" => await FakeMQ.EventBus.PublishAsync(new MessageEvent { Message = $"Action2:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" }),
                    "00" => false,
                    _ => true,
                };
                if (!result)
                    break;
            }
        }
    }
}
