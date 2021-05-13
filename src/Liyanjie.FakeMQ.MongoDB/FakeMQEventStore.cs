using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace Liyanjie.FakeMQ
{
    public class FakeMQEventStore : IFakeMQEventStore
    {
        readonly FakeMQMongoDBContext db;
        public FakeMQEventStore(IServiceProvider serviceProvider)
        {
            this.db = serviceProvider.GetRequiredService<FakeMQMongoDBContext>();
        }

        public void Add(FakeMQEvent @event)
        {
            db.Events.InsertOne(MongoDBFakeMQEvent.Wrap(@event));
        }

        public IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            return db.Events.AsQueryable()
                .Where(_ => _.Type == type && _.CreateTime > fromTime && _.CreateTime <= toTime)
                .OrderBy(_ => _.CreateTime)
                .ToList();
        }

        public class MongoDBFakeMQEvent : FakeMQEvent
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public static MongoDBFakeMQEvent Wrap(FakeMQEvent @event)
            {
                return new MongoDBFakeMQEvent
                {
                    CreateTime = @event.CreateTime,
                    Type = @event.Type,
                    Message = @event.Message,
                };
            }
        }
    }
}
