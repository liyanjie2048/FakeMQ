using MongoDB.Driver;

using static Liyanjie.FakeMQ.FakeMQEventStore;
using static Liyanjie.FakeMQ.FakeMQProcessStore;

namespace Liyanjie.FakeMQ
{
    public class FakeMQMongoDBContext
    {
        readonly IMongoClient client;
        readonly IMongoDatabase database;
        public FakeMQMongoDBContext(string connectionString)
        {
            var mongoUrl = new MongoUrlBuilder(connectionString).ToMongoUrl();
            client = new MongoClient(mongoUrl);
            database = client.GetDatabase(mongoUrl.DatabaseName);

            Seed();
        }
        void Seed()
        {
            if (!Events.Indexes.List().Any())
            {
                Events.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<MongoDBFakeMQEvent>(Builders<MongoDBFakeMQEvent>.IndexKeys.Descending(_ => _.CreateTime)),
                });
            }

            if (!Processes.Indexes.List().Any())
            {
                Processes.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<MongoDBFakeMQProcess>(Builders<MongoDBFakeMQProcess>.IndexKeys.Descending(_ => _.HandlerType)),
                });
            }
        }

        public IMongoCollection<MongoDBFakeMQEvent> Events => database.GetCollection<MongoDBFakeMQEvent>(nameof(Events));
        public IMongoCollection<MongoDBFakeMQProcess> Processes => database.GetCollection<MongoDBFakeMQProcess>(nameof(Processes));
    }
}
