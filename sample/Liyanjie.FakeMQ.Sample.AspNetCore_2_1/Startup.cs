using Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Infrastructure;
using Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Infrastructure.EventHandlers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_2_1
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
                return db.Database.ExecuteSqlCommandAsync($"DELETE FROM [FakeMQEvents] WHERE [Timestamp]<{timestamp}");
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var eventBus = app.ApplicationServices.GetRequiredService<FakeMQEventBus>();
            eventBus.SubscribeAsync<MessageEvent, MessageEventHandler>().Wait();
        }
    }
}
