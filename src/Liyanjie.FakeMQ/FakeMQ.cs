using System.Threading;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    public sealed class FakeMQ
    {
        static FakeMQEventBus eventBus;
        static CancellationTokenSource stoppingCts;
        static Task executingTask;

        public static FakeMQEventBus EventBus => eventBus;

        public static void Initialize(FakeMQEventBus eventBus)
        {
            FakeMQ.eventBus = eventBus;
            FakeMQ.stoppingCts = new CancellationTokenSource();
        }

        public static Task StartAsync(CancellationToken cancellationToken)
        {
            executingTask = eventBus.ProcessAsync(cancellationToken);
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.FromResult(0);
        }
        public static async Task StopAsync(CancellationToken cancellationToken)
        {
            if (executingTask != null)
            {
                try
                {
                    stoppingCts.Cancel();
                }
                finally
                {
                    await Task.WhenAny(executingTask, Task.Delay(-1, cancellationToken));
                }
            }
        }

        ~FakeMQ()
        {
            stoppingCts.Cancel();
        }
    }
}
