using System;
using System.Data.Entity;
using System.Linq;

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

        /// <inheritdoc />
        public void Add(FakeMQProcess process)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            if (context.FakeMQProcesses.Any(_ => _.HandlerType == process.HandlerType))
                return;
            context.FakeMQProcesses.Add(process);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public FakeMQProcess Get(string handlerType)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            return context.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.HandlerType == handlerType);
        }

        /// <inheritdoc />
        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            item.LastHandleTime = handleTime;
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(string handlerType)
        {
            using var context = new FakeMQDbContext(dbConnectionString);
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.HandlerType == handlerType);
            if (item == null)
                return;
            context.FakeMQProcesses.Remove(item);
            context.SaveChanges();
        }
    }
}
