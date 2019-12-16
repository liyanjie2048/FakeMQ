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
        /// <typeparam name="TEventStore"></typeparam>
        /// <typeparam name="TProcessStore"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQ<TEventStore, TProcessStore>(this IServiceCollection services,
            Action<FakeMQOptions> configureOptions)
            where TEventStore : class, IFakeMQEventStore
            where TProcessStore : class, IFakeMQProcessStore
        {
            services.AddTransient<IFakeMQEventStore, TEventStore>();
            services.AddTransient<IFakeMQProcessStore, TProcessStore>();
            services.Configure(configureOptions);
            services.AddSingleton<FakeMQLogger>();
            services.AddSingleton(serviceProvider => new FakeMQEventBus(serviceProvider));

            services.AddHostedService<FakeMQBackgroundService>();

            return services;
        }
    }
}
