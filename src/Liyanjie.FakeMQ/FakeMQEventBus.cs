using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NET45
using NLog;
#else
using Microsoft.Extensions.Logging;
#endif

using Polly;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FakeMQEventBus
    {
        readonly IServiceProvider serviceProvider;
#if NET45
        readonly Logger logger;
#else
        readonly Microsoft.Extensions.Logging.ILogger<FakeMQEventBus> logger;
#endif
        readonly IFakeMQEventStore eventStore;
        readonly IFakeMQProcessStore processStore;
        readonly IDictionary<Type, Type> subscriptions = new Dictionary<Type, Type>();
        readonly IDictionary<Type, object> handlerObjects = new Dictionary<Type, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
#if NET45
            this.logger = LogManager.GetCurrentClassLogger();
#else
            this.logger = serviceProvider.GetService(typeof(ILogger<FakeMQEventBus>)) as ILogger<FakeMQEventBus>;
#endif
            this.eventStore = serviceProvider.GetService(typeof(IFakeMQEventStore)) as IFakeMQEventStore;
            this.processStore = serviceProvider.GetService(typeof(IFakeMQProcessStore)) as IFakeMQProcessStore;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventStore"></param>
        /// <param name="processStore"></param>
        public FakeMQEventBus(IFakeMQEventStore eventStore, IFakeMQProcessStore processStore)
        {
            this.eventStore = eventStore;
            this.processStore = processStore;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public async Task<bool> PublishAsync<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = FakeMQEvent.GetMsgString(message),
            };
            return await TryExecuteAsync(async () => await eventStore.AddAsync(@event));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task SubscribeAsync<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (await processStore.AddAsync(new FakeMQProcess
            {
                Subscription = subscriptionId,
            }))
            {
                if (!subscriptions.ContainsKey(handlerType))
                    subscriptions.Add(handlerType, messageType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task SubscribeAsync<TEventMessage, TEventHandler>(TEventHandler handler = default)
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (await processStore.AddAsync(new FakeMQProcess
            {
                Subscription = subscriptionId,
            }))
            {
                if (!subscriptions.ContainsKey(handlerType))
                {
                    subscriptions.Add(handlerType, messageType);
                    if (handler != null)
                        handlerObjects.Add(handlerType, handler);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task UnsubscribeAsync<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (subscriptions.ContainsKey(handlerType))
            {
                subscriptions.Remove(handlerType);
                await TryExecuteAsync(async () => await processStore.DeleteAsync(subscriptionId));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task ProcessAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var timestamp = long.MaxValue;
                        foreach (var item in subscriptions.Select(_ => GetSubscriptionId(_.Value, _.Key)))
                        {
                            timestamp = Math.Min(timestamp, (await processStore.GetAsync(item)).Timestamp);
                        }
                        await eventStore.ClearAsync(timestamp);
                    }
                    catch (Exception ex)
                    {
#if NET45
                        logger.Error(ex, "出错了");
#else
                        logger.LogError(default(EventId), ex, "出错了");
#endif
                    }

                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
            });
            var tasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                tasks.Clear();

                foreach (var item in subscriptions)
                {
                    var messageType = item.Value;
                    var handlerType = item.Key;
                    var subscriptionId = GetSubscriptionId(messageType, handlerType);

                    var timestamp = (await processStore.GetAsync(subscriptionId)).Timestamp;

                    var @event = await eventStore.GetAsync(messageType.FullName, timestamp);
                    if (@event == null)
                        continue;
                    var handler = handlerObjects.ContainsKey(handlerType)
                        ? handlerObjects[handlerType]
                        : serviceProvider == null ? Activator.CreateInstance(handlerType) :
#if NET45
                            serviceProvider.GetServiceOrCreateInstance(handlerType);
#else
                            Microsoft.Extensions.DependencyInjection.ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, handlerType);
#endif
                    if (handler == null)
                        continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                        var result = await (Task<bool>)method.Invoke(handler, new[] { @event.GetMsgObject(messageType) });
                        if (result)
                            await TryExecuteAsync(async () => await processStore.UpdateAsync(subscriptionId, @event.Timestamp));
                    }));
                }

                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.FullName}>{handlerType.FullName}";
        static async Task<bool> TryExecuteAsync(Func<Task<bool>> func, int retryCount = 3)
        {
            if (func != null)
                return await Policy
                    .HandleResult<Task<bool>>(task => task.Result == false)
                    .Retry(retryCount)
                    .Execute(func);

            return false;
        }
    }
}
