using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventBus : IDisposable
    {
        readonly IServiceScope scope;
        readonly ILogger<FakeMQEventBus> logger;
        readonly IFakeMQEventStore eventStore;
        readonly IFakeMQProcessStore processStore;
        readonly IDictionary<Type, Type> subscriptions = new Dictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(
            IServiceScope scope,
            ILogger<FakeMQEventBus> logger)
        {
            this.scope = scope;
            this.logger = logger;

            this.eventStore = scope.ServiceProvider.GetRequiredService<IFakeMQEventStore>();
            this.processStore = scope.ServiceProvider.GetRequiredService<IFakeMQProcessStore>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            subscriptions.Clear();
            scope.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public bool Publish<TEventMessage>(TEventMessage message)
            where TEventMessage : IFakeMQEventMessage
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).FullName,
                Message = FakeMQEvent.GetMsgString(message),
            };
            if (TryExecute(() => eventStore.Add(@event)))
            {
                logger.LogInformation($"Publish an event:Id={@event.Id},Type={@event.Type},Message={@event.Message}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TEventMessage, TEventHandler>()
            where TEventMessage : IFakeMQEventMessage
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (processStore.Add(new FakeMQProcess
            {
                Subscription = subscriptionId,
            }))
            {
                if (!subscriptions.ContainsKey(handlerType))
                    subscriptions.Add(handlerType, messageType);

                logger.LogInformation($"Subscribe:Id={subscriptionId}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Unsubscribe<TEventMessage, TEventHandler>()
            where TEventMessage : IFakeMQEventMessage
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (subscriptions.ContainsKey(handlerType))
            {
                subscriptions.Remove(handlerType);
                TryExecute(() => processStore.Delete(subscriptionId));

                logger.LogInformation($"Unsubscribe:Id={subscriptionId}");
            }
        }

        public async Task ProcessAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                tasks.Clear();

                foreach (var subscription in subscriptions)
                {
                    var messageType = subscription.Value;
                    var handlerType = subscription.Key;
                    var subscriptionId = GetSubscriptionId(messageType, handlerType);

                    var timestamp = processStore.Get(subscriptionId)?.Timestamp ?? 0L;

                    var @event = eventStore.Get(messageType.FullName, timestamp);
                    if (@event == null)
                        continue;

                    var handler = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, handlerType);
                    if (handler == null)
                        continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method =
#if NET45
                            concreteType.GetMethod("HandleAsync");
#else
                            concreteType.GetTypeInfo().GetMethod("HandleAsync");
#endif
                        var result = await (Task<bool>)method.Invoke(handler, new[] { @event.GetMsgObject(messageType) });
                        if (result)
                            TryExecute(() => processStore.Update(subscriptionId, @event.Timestamp));

                        logger.LogInformation($"Process an event:Id={@event.Id},Handler={handlerType.FullName},Result={result}");
                    }));
                }

                await Task.WhenAll(tasks);
                await Task.Delay(1000);
            }
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.FullName}>{handlerType.FullName}";
        static bool TryExecute(Func<bool> func, int retryCount = 3)
        {
            if (func != null)
                return Policy.HandleResult<bool>(false).Retry(retryCount).Execute(func);

            return false;
        }
    }
}
