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
        public static Func<object, string> Serialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static Func<string, Type, object> Deserialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventBus"></param>
        public static void Initialize(FakeMQEventBus eventBus)
        {
            FakeMQ.eventBus = eventBus;
            FakeMQ.stoppingCts = new CancellationTokenSource();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Task StartAsync()
        {
            executingTask = eventBus.ProcessAsync(stoppingCts.Token);
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
        ~FakeMQ()
        {
            stoppingCts.Cancel();
        }
    }
}
