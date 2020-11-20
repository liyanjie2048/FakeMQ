using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

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
        public void Add(FakeMQProcess process)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            if (context.FakeMQProcesses.Any(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public FakeMQProcess Get(string handlerType)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            return context.FakeMQProcesses
                .AsNoTracking()
                .FirstOrDefault(_ => _.HandlerType == handlerType);
        }

        /// <inheritdoc />
        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            var item = context.FakeMQProcesses
                .AsTracking()
                .FirstOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(string handlerType)
        {
            using var context = FakeMQContext.GetContext(serviceProvider);
            var item = context.FakeMQProcesses
                .AsTracking()
                .FirstOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            context.SaveChanges();
        }
    }
}
