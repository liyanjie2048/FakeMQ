using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure
{
    public class FakeMQEventStore : IFakeMQEventStore
    {
        readonly DataContext context;
        public FakeMQEventStore(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }

        public async Task AddAsync(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }
        public void Add(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            return await context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime < toTime)
                .OrderBy(_ => _.CreateTime)
                .ToListAsync();
        }
        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            return context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime < toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }
    }
}
