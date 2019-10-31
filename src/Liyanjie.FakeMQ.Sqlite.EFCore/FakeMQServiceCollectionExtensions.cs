using System;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class FakeMQServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQSqliteEFCore(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            services.AddDbContext<FakeMQContext>(optionsAction, ServiceLifetime.Singleton);
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);

            return services;
        }
    }
}
