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
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQSqliteEFCore(this IServiceCollection services, string connectionString = @"Database=.\FakeMQ.sqlite")
        {
            services.AddDbContext<FakeMQContext>(options =>
            {
                options.UseSqlite(connectionString);
            }, ServiceLifetime.Singleton);
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);

            return services;
        }
    }
}
