﻿using System;

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
            services.AddTransient<IFakeMQEventStore, TEventStore>();
            services.AddTransient<IFakeMQProcessStore, TProcessStore>();
            services.AddSingleton(serviceProvider => new FakeMQEventBus(serviceProvider.CreateScope(), serviceProvider.GetService<ILogger<FakeMQEventBus>>()));
            services.AddHostedService<FakeMQBackgroundService>();

            FakeMQDefaults.JsonSerialize = jsonSerialize;
            FakeMQDefaults.JsonDeserialize = jsonDeserialize;

            return services;
        }
    }
}