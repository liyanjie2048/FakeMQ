using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventStore : IFakeMQEventStore
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventStore(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task AddAsync(FakeMQEvent @event)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            context.FakeMQEvents.Add(@event);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public void Add(FakeMQEvent @event)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            context.FakeMQEvents.Add(@event);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FakeMQEvent>> GetAsync(string type, long startTimestamp, long endTimestamp)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<FakeMQContext>();
            return await context.FakeMQEvents
                .AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > startTimestamp && _.Timestamp <= endTimestamp)
                .OrderBy(_ => _.Timestamp)
                .ToListAsync();
        }
    }
}
