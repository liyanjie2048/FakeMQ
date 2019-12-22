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

        public async Task AddAsync(FakeMQProcess process)
        {
            if (await context.FakeMQProcesses.AnyAsync(_ => _.HandlerType == process.HandlerType))
                return;

            context.FakeMQProcesses.Add(process);

            await context.SaveChangesAsync();
        }
        public void Add(FakeMQProcess process)
        {
            if (context.FakeMQProcesses.Any(_ => _.HandlerType == process.HandlerType))
                return;

            context.FakeMQProcesses.Add(process);

            context.SaveChanges();
        }
        public async Task<FakeMQProcess> GetAsync(string handlerType)
        {
            return await context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
        }
        public FakeMQProcess Get(string subscription)
        {
            return context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.HandlerType == subscription);
        }
        public async Task UpdateAsync(string handlerType, DateTimeOffset handleTime)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;

            item.LastHandleTime = handleTime;

            await context.SaveChangesAsync();
        }
        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;

            item.LastHandleTime = handleTime;

            context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string handlerType)
        {
            var item = await context.FakeMQProcesses.SingleOrDefaultAsync(_ => _.HandlerType == handlerType);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            await context.SaveChangesAsync();
        }

        public void Delete(string handlerType)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;

            context.FakeMQProcesses.Remove(item);

            context.SaveChanges();
        }
    }
}
