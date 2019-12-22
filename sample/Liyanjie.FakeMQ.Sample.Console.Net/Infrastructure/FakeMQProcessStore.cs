using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQProcessStore : IFakeMQProcessStore
    {
        readonly string dbConnectionString;
        public FakeMQProcessStore(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
        }

        public async Task AddAsync(FakeMQProcess process)
        {
            using var context = new DataContext(dbConnectionString);
            if (await context.FakeMQProcesses.AnyAsync(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            await context.SaveChangesAsync();
        }
        public void Add(FakeMQProcess process)
        {
            using var context = new DataContext(dbConnectionString);
            if (context.FakeMQProcesses.Any(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            context.SaveChanges();
        }
        public async Task<FakeMQProcess> GetAsync(string handlerType)
        {
            using var context = new DataContext(dbConnectionString);
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
        }
        public FakeMQProcess Get(string handlerType)
        {
            using var context = new DataContext(dbConnectionString);
            return context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.HandlerType == handlerType);
        }
        public async Task UpdateAsync(string handlerType, DateTimeOffset handleTime)
        {
            using var context = new DataContext(dbConnectionString);
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            await context.SaveChangesAsync();
        }
        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            using var context = new DataContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string handlerType)
        {
            using var context = new DataContext(dbConnectionString);
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            await context.SaveChangesAsync();
        }

        public void Delete(string handlerType)
        {
            using var context = new DataContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            context.SaveChanges();
        }
    }
}
