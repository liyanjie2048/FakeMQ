using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQProcessStore : IFakeMQProcessStore
    {
        readonly FakeMQContext context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public FakeMQProcessStore(FakeMQContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            context.Dispose();
        }

        /// <inheritdoc />
        public async Task AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return;

            context.FakeMQProcesses.Add(process);

            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Add(FakeMQProcess process)
        {
            if (context.FakeMQProcesses.Any(_ => _.Subscription == process.Subscription))
                return;

            context.FakeMQProcesses.Add(process);

            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            return await context.FakeMQProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Subscription == subscription);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string subscription, long timestamp)
        {
            var item = await context.FakeMQProcesses
                .AsTracking()
                .FirstOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            item.Timestamp = timestamp;

            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string subscription)
        {
            var item = await context.FakeMQProcesses
                .AsTracking()
                .FirstOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Delete(string subscription)
        {
            var item = context.FakeMQProcesses
                .AsTracking()
                .FirstOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            context.SaveChanges();
        }
    }
}
