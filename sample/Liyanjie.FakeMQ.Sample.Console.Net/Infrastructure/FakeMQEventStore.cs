using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
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
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, long startTimestamp, long endTimestamp)
        {
            return await context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > startTimestamp && _.Timestamp < endTimestamp)
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
            var sql = $"DELETE FROM [FakeMQEvents] WHERE [Timestamp]<{timestamp}";
            context.Database.ExecuteSqlCommand(sql);

            await Task.FromResult(0);
        }
    }
}
