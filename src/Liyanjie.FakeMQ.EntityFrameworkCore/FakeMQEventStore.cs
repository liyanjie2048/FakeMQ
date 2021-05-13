using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventStore : IFakeMQEventStore
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventStore(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public void Add(FakeMQEvent @event)
        {
            using var context = FakeMQDbContext.GetContext(serviceProvider);
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = FakeMQDbContext.GetContext(serviceProvider);
            return context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime <= toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }
    }
}
