using Liyanjie.FakeMQ.Sample.AspNetCore.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EventHandlers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace Liyanjie.FakeMQ.Sample.AspNetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddDbContext<SqliteContext>(builder =>
            {
                builder.UseSqlite(@"Data Source=.\Database.sqlite");
            });

            services.AddFakeMQ<EventStore, ProcessStore>(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var eventBus = app.ApplicationServices.GetRequiredService<FakeMQEventBus>();
            eventBus.Subscribe<MessageEvent, MessageEventHandler>();

            app.UseMvc();
        }
    }
}
