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
        static async Task ShowMessagesAsync()
        {
            using var db = GetDataContext();
            System.Console.WriteLine("################################");
            foreach (var item in await db.Messages.ToListAsync())
            {
                System.Console.WriteLine($"{item.Id}=>{item.Content}");
            }
            System.Console.WriteLine("################################");
            System.Console.WriteLine();
        }
        static DataContext GetDataContext() => new DataContext(ConfigurationManager.ConnectionStrings["Sqlite"].ConnectionString);

        static async Task Main(string[] args)
        {
            FakeMQ.Initialize(new FakeMQOptions
            {
                Serialize = JsonConvert.SerializeObject,
                Deserialize = JsonConvert.DeserializeObject,
                GetEventStore = serviceProvider => new FakeMQEventStore(GetDataContext()),
                GetProcessStore = serviceProvider => new FakeMQProcessStore(GetDataContext())
            }, new FakeMQLogger());
            await FakeMQ.EventBus.SubscribeAsync<MessageEvent, MessageEventHandler>(new MessageEventHandler(GetDataContext()));

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
