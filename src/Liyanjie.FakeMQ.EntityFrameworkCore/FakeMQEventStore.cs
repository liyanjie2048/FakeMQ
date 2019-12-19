using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task AddAsync(FakeMQEvent @event)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Add(FakeMQEvent @event)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            return await context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime <= toTime)
                .OrderBy(_ => _.CreateTime)
                .ToListAsync();
        }

        /// <inheritdoc />
        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            return context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime <= toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }
    }
}
