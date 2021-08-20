using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FakeMQ
    {
        static ILogger<FakeMQ> logger;
        static FakeMQOptions options;
        static FakeMQEventBus eventBus;
        static CancellationTokenSource stoppingCts;
        static Task executingTask;

        /// <summary>
        /// 
        /// </summary>
        public static FakeMQEventBus EventBus => eventBus;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            FakeMQ.logger = serviceProvider.GetRequiredService<ILogger<FakeMQ>>();
            FakeMQ.options = serviceProvider.GetRequiredService<IOptions<FakeMQOptions>>().Value;
            FakeMQ.eventBus = serviceProvider.GetRequiredService<FakeMQEventBus>();
            FakeMQ.stoppingCts = new CancellationTokenSource();
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsProcessing { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public static DateTimeOffset LastLoopTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Task StartAsync()
        {
            executingTask = ProcessAsync(stoppingCts.Token);
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task StopAsync()
        {
            if (executingTask == null)
                return;

            try
            {
                stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(executingTask, Task.Delay(-1));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        static async Task ProcessAsync(CancellationToken stoppingToken)
        {
            if (IsProcessing)
                return;

            logger.LogInformation("FakeMQ process start.");

            while (!stoppingToken.IsCancellationRequested)
            {
                IsProcessing = true;
                LastLoopTime = DateTimeOffset.Now;

                await eventBus.HandleAsync();

                await Task.Delay(options.LoopTimeSpan);
            }

            IsProcessing = false;
            logger.LogInformation("FakeMQ process stop.");
        }

        /// <summary>
        /// 
        /// </summary>
        ~FakeMQ()
        {
            stoppingCts.Cancel();
        }
    }
}
