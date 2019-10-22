using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure
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
