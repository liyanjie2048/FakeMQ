using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

using static Liyanjie.FakeMQ.FakeMQEventStore;
using static Liyanjie.FakeMQ.FakeMQProcessStore;

namespace Liyanjie.FakeMQ
{
    public class FakeMQMongoDBContextSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<FakeMQMongoDBContext>();

            if (!context.Events.Indexes.List().Any())
            {
                context.Events.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<MongoDBFakeMQEvent>(Builders<MongoDBFakeMQEvent>.IndexKeys.Descending(_ => _.CreateTime)),
                });
            }

            if (!context.Processes.Indexes.List().Any())
            {
                context.Processes.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<MongoDBFakeMQProcess>(Builders<MongoDBFakeMQProcess>.IndexKeys.Descending(_ => _.HandlerType)),
                });
            }
        }
    }
}
