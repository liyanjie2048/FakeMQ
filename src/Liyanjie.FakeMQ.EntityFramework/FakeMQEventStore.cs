using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventStore : IFakeMQEventStore
    {
        readonly string dbConnectionString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConnectionString"></param>
        public FakeMQEventStore(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task AddAsync(FakeMQEvent @event)
        {
            using var context = new FakeMQContext(dbConnectionString);
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public void Add(FakeMQEvent @event)
        {
            using var context = new FakeMQContext(dbConnectionString);
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = new FakeMQContext(dbConnectionString);
            return await context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime < toTime)
                .OrderBy(_ => _.CreateTime)
                .ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = new FakeMQContext(dbConnectionString);
            return context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime < toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }
    }
}
