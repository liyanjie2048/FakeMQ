﻿using System;

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
            Action<FakeMQOptions> configureOptions = null)
            where TEventStore : class, IFakeMQEventStore
            where TProcessStore : class, IFakeMQProcessStore
        {
            if (configureOptions != null)
                services.Configure(configureOptions);
            services.AddSingleton<FakeMQEventBus>();
            services.AddSingleton<IFakeMQEventStore, TEventStore>();
            services.AddSingleton<IFakeMQProcessStore, TProcessStore>();

            services.AddHostedService<FakeMQBackgroundService>();

            return services;
        }
    }
}
