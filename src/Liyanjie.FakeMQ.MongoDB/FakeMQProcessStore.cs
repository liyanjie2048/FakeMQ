using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace Liyanjie.FakeMQ
{
    public class FakeMQProcessStore : IFakeMQProcessStore
    {
        readonly FakeMQMongoDBContext db;
        public FakeMQProcessStore(IServiceProvider serviceProvider)
        {
            this.db = serviceProvider.GetRequiredService<FakeMQMongoDBContext>();
        }

        public void Add(FakeMQProcess process)
        {
            if (db.Processes.AsQueryable().Any(_ => _.HandlerType == process.HandlerType))
                return;
            db.Processes.InsertOne(MongoDBFakeMQProcess.Wrap(process));
        }

        public FakeMQProcess Get(string handlerType)
        {
            return db.Processes.AsQueryable().FirstOrDefault(_ => _.HandlerType == handlerType);
        }

        public void Update(string handlerType, DateTimeOffset handleTime)
        {
            db.Processes
                .UpdateOne(_ => _.HandlerType == handlerType, Builders<MongoDBFakeMQProcess>.Update
                    .Set(_ => _.LastHandleTime, handleTime)
                );
        }

        public void Delete(string handlerType)
        {
            db.Processes.DeleteOne(_ => _.HandlerType == handlerType);
        }

        public class MongoDBFakeMQProcess : FakeMQProcess
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public static MongoDBFakeMQProcess Wrap(FakeMQProcess process)
            {
                return new MongoDBFakeMQProcess
                {
                    HandlerType = process.HandlerType,
                    LastHandleTime = process.LastHandleTime,
                };
            }
        }
    }
}
