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
            services.AddLogging();
            services.AddDbContext<SqliteContext>();

            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(_ => JsonSerializer.Serialize(_), (input, type) => JsonSerializer.Deserialize(input, type));
            services.AddSingleton<MessageEventHandler>();
        }
        static void InitializeDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<SqliteContext>();
            context.Database.EnsureCreated();
        }
        static bool ShowMessages(IServiceProvider serviceProvider)
        {
            System.Console.WriteLine("################################");
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<SqliteContext>();
            foreach (var item in context.Messages.ToList())
            {
                System.Console.WriteLine($"{item.Id}=>{item.Content}");
            }
            System.Console.WriteLine("################################");
            System.Console.WriteLine();

            return true;
        }

        static async Task Main(string[] args)
        {
            var stoppingCts = new CancellationTokenSource();

            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            InitializeDatabase(serviceProvider);

            FakeMQ.Initialize(serviceProvider.GetService<FakeMQEventBus>());
            FakeMQ.EventBus.Subscribe<MessageEvent, MessageEventHandler>();
            await FakeMQ.StartAsync(stoppingCts.Token);

            while (true)
            {
                System.Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                var result = input switch
                {
                    "0" => ShowMessages(serviceProvider),
                    "1" => FakeMQ.EventBus.Publish(new MessageEvent { Message = $"Action1:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" }),
                    "2" => FakeMQ.EventBus.Publish(new MessageEvent { Message = $"Action2:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" }),
                    "00" => false,
                    _ => true,
                };
                if (!result)
                    break;
            }
        }
    }
}
