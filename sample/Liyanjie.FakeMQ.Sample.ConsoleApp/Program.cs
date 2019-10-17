using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.ConsoleApp.Infrastructure;
using Liyanjie.FakeMQ.Sample.ConsoleApp.Infrastructure.EventHandlers;
using Liyanjie.FakeMQ.Sample.ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            var stoppingCts = new CancellationTokenSource();

            services.AddLogging();
            services.AddDbContext<SqliteContext>(builder =>
            {
                builder.UseSqlite(@"Data Source=.\Database.sqlite");
            });

            services.AddFakeMQ<EventStore, ProcessStore>(_ => JsonSerializer.Serialize(_), (input, type) => JsonSerializer.Deserialize(input, type));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<SqliteContext>();
            context.Database.EnsureCreated();

            FakeMQ.Initialize(serviceProvider.GetService<FakeMQEventBus>());
            FakeMQ.EventBus.Subscribe<MessageEvent, MessageEventHandler>();

            await FakeMQ.StartAsync(stoppingCts.Token);

            bool ShowMessages()
            {
                Console.WriteLine("################################");
                using var scope = serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetService<SqliteContext>();
                foreach (var item in context.Messages.ToList())
                {
                    Console.WriteLine($"{item.Id}=>{item.Content}");
                }
                Console.WriteLine("################################");
                Console.WriteLine();

                return true;
            }

            while (true)
            {
                Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                var result = input switch
                {
                    "0" => ShowMessages(),
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
