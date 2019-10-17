using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQBackgroundService : BackgroundService
    {
        readonly FakeMQEventBus eventBus;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventBus"></param>
        public FakeMQBackgroundService(FakeMQEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await eventBus.ProcessAsync(stoppingToken);
        }
    }
}
