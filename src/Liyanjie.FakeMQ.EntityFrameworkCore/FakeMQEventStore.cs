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
        readonly FakeMQContext context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public FakeMQEventStore(FakeMQContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal static Func<FakeMQContext, long, Task> CleanEvent { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            context.Dispose();
        }

        /// <inheritdoc />
        public async Task AddAsync(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Add(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, long startTimestamp, long endTimestamp)
        {
            return await context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > startTimestamp && _.Timestamp <= endTimestamp)
                .OrderBy(_ => _.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task CleanAsync(long timestamp)
        {
            if (CleanEvent != null)
            {
                await CleanEvent.Invoke(context, timestamp);
                await context.SaveChangesAsync();
            }
        }
    }
}
