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

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            context.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task AddAsync(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startTimestamp"></param>
        /// <param name="endTimestamp"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, long startTimestamp, long endTimestamp)
        {
            return await context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > startTimestamp && _.Timestamp <= endTimestamp)
                .OrderBy(_ => _.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
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
