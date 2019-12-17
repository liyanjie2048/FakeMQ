using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQProcessStore : IFakeMQProcessStore
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQProcessStore(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task AddAsync(FakeMQProcess process)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            if (await context.FakeMQProcesses.AnyAsync(_ => _.Subscription == process.Subscription))
                return;
            context.FakeMQProcesses.Add(process);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Add(FakeMQProcess process)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            if (context.FakeMQProcesses.Any(_ => _.Subscription == process.Subscription))
                return;
            context.FakeMQProcesses.Add(process);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<FakeMQProcess> GetAsync(string subscription)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            return await context.FakeMQProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Subscription == subscription);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string subscription, long timestamp)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
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
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
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
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
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
