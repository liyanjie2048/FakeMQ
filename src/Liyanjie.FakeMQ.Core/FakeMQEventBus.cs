using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Polly;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventBus
    {
        readonly IServiceProvider serviceProvider;
        readonly IFakeMQEventStore eventStore;
        readonly IFakeMQProcessStore processStore;
        readonly IDictionary<Type, Type> subscriptions = new Dictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.eventStore = serviceProvider.GetService(typeof(IFakeMQEventStore)) as IFakeMQEventStore;
            this.processStore = serviceProvider.GetService(typeof(IFakeMQProcessStore)) as IFakeMQProcessStore;
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
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public FakeMQEventBus Subscribe<TEventMessage, TEventHandler>()
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
            }

            return this;
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

                    var handler = serviceProvider.GetService(handlerType);
                    if (handler == null)
                        continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method = concreteType.GetTypeInfo().GetMethod("HandleAsync");
                        var result = await (Task<bool>)method.Invoke(handler, new[] { @event.GetMsgObject(messageType) });
                        if (result)
                            TryExecute(() => processStore.Update(subscriptionId, @event.Timestamp));
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
