using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQProcessStore : IFakeMQProcessStore
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

        public async Task AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return;

            context.FakeMQProcesses.Add(process);

            await context.SaveChangesAsync();
        }
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
        }
        public async Task UpdateAsync(string subscription, long timestamp)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            item.Timestamp = timestamp;

            await context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string subscription)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            await context.SaveChangesAsync();
        }
    }
}
