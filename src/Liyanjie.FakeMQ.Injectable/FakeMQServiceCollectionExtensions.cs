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
        /// <param name="jsonSerialize"></param>
        /// <param name="jsonDeserialize"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQ<TEventStore, TProcessStore>(this IServiceCollection services,
            Func<object, string> jsonSerialize,
            Func<string, Type, object> jsonDeserialize)
            where TEventStore : class, IFakeMQEventStore
            where TProcessStore : class, IFakeMQProcessStore
        {
            FakeMQ.Serialize = jsonSerialize ?? throw new ArgumentNullException(nameof(jsonSerialize));
            FakeMQ.Deserialize = jsonDeserialize ?? throw new ArgumentNullException(nameof(jsonDeserialize));

            services.AddTransient<IFakeMQEventStore, TEventStore>();
            services.AddTransient<IFakeMQProcessStore, TProcessStore>();
            services.AddSingleton(serviceProvider =>
            {
                var eventBus = new FakeMQEventBus(serviceProvider);

                FakeMQ.Initialize(eventBus);

                return eventBus;
            });

            services.AddHostedService<FakeMQBackgroundService>();

            return services;
        }
    }
}
