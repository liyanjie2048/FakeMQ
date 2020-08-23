using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQProcessStore : IFakeMQProcessStore
    {
        readonly string dbConnectionString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConnectionString"></param>
        public FakeMQProcessStore(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public async Task AddAsync(FakeMQProcess process)
        {
            using var context = new FakeMQContext(dbConnectionString);
            if (await context.FakeMQProcesses.AnyAsync(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        public void Add(FakeMQProcess process)
        {
            using var context = new FakeMQContext(dbConnectionString);
            if (context.FakeMQProcesses.Any(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            context.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public async Task<FakeMQProcess> GetAsync(string handlerType)
        {
            using var context = new FakeMQContext(dbConnectionString);
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public FakeMQProcess Get(string handlerType)
        {
            using var context = new FakeMQContext(dbConnectionString);
            return context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.HandlerType == handlerType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="handleTime"></param>
        /// <returns></returns>
        public async Task UpdateAsync(string handlerType, DateTimeOffset handleTime)
        {
            using var context = new FakeMQContext(dbConnectionString);
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="handleTime"></param>
        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            using var context = new FakeMQContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string handlerType)
        {
            using var context = new FakeMQContext(dbConnectionString);
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        public void Delete(string handlerType)
        {
            using var context = new FakeMQContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            context.SaveChanges();
        }
    }
}
