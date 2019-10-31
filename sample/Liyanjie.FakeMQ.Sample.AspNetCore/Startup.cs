using Liyanjie.FakeMQ.Sample.AspNetCore.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EventHandlers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.AspNetCore
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

            services.AddFakeMQSqliteEFCore();

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
