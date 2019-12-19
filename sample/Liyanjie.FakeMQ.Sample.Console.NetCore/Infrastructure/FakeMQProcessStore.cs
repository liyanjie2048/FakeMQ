using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure
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
        public void Add(FakeMQProcess process)
        {
            if (context.FakeMQProcesses.Any(_ => _.Subscription == process.Subscription))
                return;

            context.FakeMQProcesses.Add(process);

            context.SaveChanges();
        }
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
        }
        public FakeMQProcess Get(string subscription)
        {
            return context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.Subscription == subscription);
        }
        public async Task UpdateAsync(string subscription, DateTimeOffset handleTime)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            item.LastHandleTime = handleTime;

            await context.SaveChangesAsync();
        }
        public void Update(string subscription, DateTimeOffset handleTime)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return;

            item.LastHandleTime = handleTime;

            context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string subscription)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            await context.SaveChangesAsync();
        }

        public void Delete(string subscription)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            context.SaveChanges();
        }
    }
}
