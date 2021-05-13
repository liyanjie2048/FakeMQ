using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

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

        /// <inheritdoc />
        public void Add(FakeMQEvent @event)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            return context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime < toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }
    }
}
