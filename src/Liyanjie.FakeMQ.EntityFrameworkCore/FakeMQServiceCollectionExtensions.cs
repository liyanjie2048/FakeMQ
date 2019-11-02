using System;
using System.Threading.Tasks;

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
        /// <param name="configureOptions"></param>
        /// <param name="clearEventStore"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQWithEFCore(this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureOptions,
            Func<FakeMQContext, long, Task> clearEventStore)
        {
            FakeMQEventStore.ClearEventStore = clearEventStore;
            services.AddDbContext<FakeMQContext>(configureOptions, ServiceLifetime.Transient, ServiceLifetime.Singleton);
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);

            return services;
        }
    }
}
