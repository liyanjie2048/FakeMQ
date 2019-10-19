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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return eventBus.ProcessAsync(stoppingToken);
        }
    }
}
#if NET451 || NETSTANDARD1_5
namespace Microsoft.Extensions.Hosting
{
    using System;

    public abstract class BackgroundService
    {
        Task executingTask;

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            executingTask = ExecuteAsync(cancellationToken);

            if (executingTask.IsCompleted)
            {
                return executingTask;
            }

            return Task.FromResult(0);
        }
    }
}
#endif