using System;
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

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            context.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public async Task AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return;

            context.FakeMQProcesses.Add(process);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            return await context.FakeMQProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Subscription == subscription);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
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
    }
}
