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
            this.context?.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return true;

            context.FakeMQProcesses.Add(process);

            return await SaveAsync();
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
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(string subscription, long timestamp)
        {
            var item = await context.FakeMQProcesses
                .AsTracking()
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            item.Timestamp = timestamp;

            return await SaveAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string subscription)
        {
            var item = await context.FakeMQProcesses
                .AsTracking()
                .SingleOrDefaultAsync(_ => _.Subscription == subscription);
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
