using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure;
using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers;
using Liyanjie.FakeMQ.Sample.Console.Net.Models;

using Newtonsoft.Json;

namespace Liyanjie.FakeMQ.Sample.Console.Net
{
    class Program
    {
        internal static string ConnectionString => ConfigurationManager.ConnectionStrings["Sqlite"].ConnectionString;

        static async Task ShowMessagesAsync()
        {
            using var context = new DataContext(ConnectionString);
            System.Console.WriteLine("################################");
            foreach (var item in await context.Messages.ToListAsync())
            {
                System.Console.WriteLine($"{item.Id}=>{item.Content}");
            }
            System.Console.WriteLine("################################");
            System.Console.WriteLine();
        }
        static async Task Main(string[] args)
        {
            var options = new FakeMQOptions();
            var logger = new FakeMQLogger();
            var eventBus = new FakeMQEventBus(options, logger, new FakeMQEventStore(ConnectionString), new FakeMQProcessStore(ConnectionString));
            FakeMQ.Initialize(options, logger, eventBus);
            await FakeMQ.EventBus.RegisterEventHandlerAsync<MessageEvent, MessageEventHandler>();

            await FakeMQ.StartAsync();

            while (true)
            {
                System.Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                switch (input)
                {
                    case "0":
                        await ShowMessagesAsync();
                        break;
                    case "1":
                        await FakeMQ.EventBus.PublishEventAsync(new MessageEvent { Message = $"Action1:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" });
                        break;
                    case "2":
                        await FakeMQ.EventBus.PublishEventAsync(new MessageEvent { Message = $"Action2:{DateTimeOffset.Now.ToString("yyyyMMddHHmmssfffffffzzzz")}" });
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
