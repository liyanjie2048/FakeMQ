using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQProcessStore : IFakeMQProcessStore, IDisposable
    {
        readonly DataContext context;
        public FakeMQProcessStore(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }

        public async Task<bool> AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return true;

            context.FakeMQProcesses.Add(process);

            return await SaveAsync();
        }
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
        }
        public async Task<bool> UpdateAsync(string subscription, long timestamp)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            item.Timestamp = timestamp;

            return await SaveAsync();
        }
        public async Task<bool> DeleteAsync(string subscription)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            context.FakeMQProcesses.Remove(item);

            return await SaveAsync();
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
