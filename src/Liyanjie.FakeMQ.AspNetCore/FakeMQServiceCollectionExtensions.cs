using System;

using Liyanjie.FakeMQ;

using Microsoft.Extensions.Logging;

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
        /// <param name="jsonSerialize"></param>
        /// <param name="jsonDeserialize"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQ<TEventStore, TProcessStore>(this IServiceCollection services,
            Func<object, string> jsonSerialize,
            Func<string, Type, object> jsonDeserialize)
            where TEventStore : class, IFakeMQEventStore
            where TProcessStore : class, IFakeMQProcessStore
        {
            FakeMQDefaults.Serialize = jsonSerialize ?? throw new ArgumentNullException(nameof(jsonSerialize));
            FakeMQDefaults.Deserialize = jsonDeserialize ?? throw new ArgumentNullException(nameof(jsonDeserialize));

            services.AddTransient<IFakeMQEventStore, TEventStore>();
            services.AddTransient<IFakeMQProcessStore, TProcessStore>();
            services.AddSingleton(serviceProvider => new FakeMQEventBus(
#if NETSTANDARD2_0
                serviceProvider.CreateScope().ServiceProvider
#else
                serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider
#endif
            ));

#if NETSTANDARD2_0
            services.AddHostedService<FakeMQBackgroundService>();
#endif
            return services;
        }
    }
}
