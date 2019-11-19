﻿using System;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;

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
        /// <param name="configureDbContextOptions"></param>
        /// <param name="configureFakeMQOptions"></param>
        /// <param name="clearEventStore"></param>
        /// <returns></returns>
        public static IServiceCollection AddFakeMQWithEFCore(this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureDbContextOptions,
            Action<FakeMQOptions> configureFakeMQOptions,
            Func<FakeMQContext, long, Task> clearEventStore = null)
        {
            FakeMQEventStore.ClearEventStore = clearEventStore;
            services.AddDbContext<FakeMQContext>(configureDbContextOptions, ServiceLifetime.Transient, ServiceLifetime.Singleton);
            services.AddFakeMQ<FakeMQEventStore, FakeMQProcessStore>(configureFakeMQOptions);

            return services;
        }
    }
}
