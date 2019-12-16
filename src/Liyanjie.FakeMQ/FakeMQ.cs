using System;
using System.Threading;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FakeMQ
    {
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
        /// <param name="options"></param>
        /// <param name="eventBus"></param>
        public static void Initialize(FakeMQOptions options, FakeMQEventBus eventBus = null)
        {
            FakeMQ.options = options;
            FakeMQ.eventBus = eventBus ?? new FakeMQEventBus(options);
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
            if (executingTask != null)
            {
                try
                {
                    stoppingCts.Cancel();
                }
                finally
                {
                    await Task.WhenAny(executingTask, Task.Delay(-1));
                }
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

            options.Log("Information", "FakeMQ process start.");

            while (!stoppingToken.IsCancellationRequested)
            {
                IsProcessing = true;
                LastLoopTime = DateTimeOffset.Now;

                await eventBus.HandleAsync();

                await Task.Delay(options.EventHandlingLoopTimeSpan);
            }

            IsProcessing = false;
            options.Log("Information", "FakeMQ process stop.");
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
