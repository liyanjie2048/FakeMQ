using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQEventStore : IFakeMQEventStore, IDisposable
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

        public async Task<bool> AddAsync(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            return await SaveAsync();
        }

        public async Task<FakeMQEvent> GetAsync(string type, long timestamp)
        {
            return await context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > timestamp)
                .OrderBy(_ => _.Timestamp)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public async Task ClearAsync(long timestamp)
        {
            var sql = $"DELETE FROM [FakeMQEvents] WHERE [Timestamp]<{timestamp}";
            context.Database.ExecuteSqlCommand(sql);

            await Task.FromResult(0);
        }

        async Task<bool> SaveAsync()
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch { }
            return false;
        }
    }
}
