using System;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers
{
    public class MessageEventHandler : IFakeMQEventHandler<MessageEvent>
    {
        readonly string dbConnectionString;
        public MessageEventHandler(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
        }

        public async Task<bool> HandleAsync(MessageEvent @event)
        {
            try
            {
                using var context = new DataContext(dbConnectionString);
                context.Messages.Add(new Message
                {
                    Content = @event.Message,
                });
                await context.SaveChangesAsync();
            }
            catch { }
            return true;
        }
    }
}
