using System;

using Liyanjie.FakeMQ;

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
        /// <param name="mongoDBConnectionString"></param>
        /// <param name="configureFakeMQOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQWithMongoDB(this IServiceCollection services,
            string mongoDBConnectionString,
            Action<FakeMQOptions> configureFakeMQOptions = null)
        {
            services.AddTransient(serivceProvider => new FakeMQMongoDBContext(mongoDBConnectionString));
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(configureFakeMQOptions);

            return services;
        }
    }
}
