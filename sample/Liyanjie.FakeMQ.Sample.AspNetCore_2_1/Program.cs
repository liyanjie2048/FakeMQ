using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_2_1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddNLog();
                });
    }
}
