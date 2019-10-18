using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure;
using Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers;
using Liyanjie.FakeMQ.Sample.Console.Net.Models;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace Liyanjie.FakeMQ.Sample.Console.Net
{
    class Program
    {
        static void ConfigureServices(ServiceCollection services)
        {
            services.AddScoped(sp => new SqlCeContext(ConfigurationManager.ConnectionStrings["SqlServerCe"].ConnectionString));

            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);
        }
        static bool ShowMessages(IServiceProvider serviceProvider)
        {
            System.Console.WriteLine("################################");
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = scope.ServiceProvider.GetService<SqlCeContext>();
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
