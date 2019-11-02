using Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Infrastructure;
using Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Infrastructure.EventHandlers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_3_0
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddDbContext<DataContext>(builder =>
            {
                builder.UseSqlite(@"Data Source=.\Database.sqlite");
            });

            services.AddFakeMQWithEFCore(options =>
            {
                options.UseSqlite(@"Data Source=.\FakeMQ.sqlite", sqlite => sqlite.MigrationsAssembly(typeof(Startup).Assembly.FullName));
            },
            (db, timestamp) =>
            {
                return db.Database.ExecuteSqlRawAsync($"DELETE FROM [FakeMQEvents] WHERE [Timestamp]<{timestamp}");
            });

            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            var eventBus = app.ApplicationServices.GetRequiredService<FakeMQEventBus>();
            eventBus.SubscribeAsync<MessageEvent, MessageEventHandler>().Wait();
        }
    }
}
