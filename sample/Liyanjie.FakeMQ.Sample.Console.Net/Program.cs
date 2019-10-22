using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure;
using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers;
using Liyanjie.FakeMQ.Sample.Console.Net.Models;

using Newtonsoft.Json;

namespace Liyanjie.FakeMQ.Sample.Console.Net
{
    class Program
    {
        static async Task<bool> ShowMessagesAsync()
        {
            using var db = new DataContext(ConfigurationManager.ConnectionStrings["Sqlite"].ConnectionString);
            System.Console.WriteLine("################################");
            foreach (var item in await db.Messages.ToListAsync())
            {
                System.Console.WriteLine($"{item.Id}=>{item.Content}");
            }
            System.Console.WriteLine("################################");
            System.Console.WriteLine();

            return true;
        }

        static async Task Main(string[] args)
        {
            var db = new DataContext(ConfigurationManager.ConnectionStrings["Sqlite"].ConnectionString);

            FakeMQ.Serialize = JsonConvert.SerializeObject;
            FakeMQ.Deserialize = JsonConvert.DeserializeObject;

            FakeMQ.Initialize(new FakeMQEventBus(new FakeMQEventStore(db), new FakeMQProcessStore(db)));
            await FakeMQ.EventBus.SubscribeAsync<MessageEvent, MessageEventHandler>(new MessageEventHandler(db));

            await FakeMQ.StartAsync();

            while (true)
            {
                System.Console.WriteLine("INPUT:");
                var input = System.Console.ReadLine();
                var result = input switch
                {
                    "0" => await ShowMessagesAsync(),
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
