using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQBackgroundService : IHostedService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventBus"></param>
        public FakeMQBackgroundService(FakeMQEventBus eventBus)
        {
            FakeMQ.Initialize(eventBus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken) => FakeMQ.StartAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken) => FakeMQ.StopAsync();
    }
}