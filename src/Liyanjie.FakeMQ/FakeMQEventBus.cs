using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#if NETSTANDARD2_0||NETSTANDARD2_1
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endif

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FakeMQEventBus
    {
        readonly IDictionary<Type, Type> subscriptions = new Dictionary<Type, Type>();
        readonly IDictionary<Type, object> handlerObjects = new Dictionary<Type, object>();
        readonly IDictionary<string, DateTimeOffset> processTimes = new Dictionary<string, DateTimeOffset>();

        readonly FakeMQOptions options;
        readonly FakeMQLogger logger;
        readonly IFakeMQEventStore eventStore;
        readonly IFakeMQProcessStore processStore;

#if NETSTANDARD2_0 || NETSTANDARD2_1
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.options = serviceProvider.GetRequiredService<IOptions<FakeMQOptions>>().Value;
            this.logger = serviceProvider.GetRequiredService<FakeMQLogger>();
            this.eventStore = serviceProvider.GetRequiredService<IFakeMQEventStore>();
            this.processStore = serviceProvider.GetRequiredService<IFakeMQProcessStore>();
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="eventStore"></param>
        /// <param name="processStore"></param>
        public FakeMQEventBus(
            FakeMQOptions options,
            FakeMQLogger logger,
            IFakeMQEventStore eventStore,
            IFakeMQProcessStore processStore)
        {
            this.options = options;
            this.logger = logger;
            this.eventStore = eventStore;
            this.processStore = processStore;
        }

        /// <summary>
        /// 所有订阅
        /// </summary>
        public IReadOnlyDictionary<Type, Type> Subscriptions => new ReadOnlyDictionary<Type, Type>(subscriptions);

        /// <summary>
        /// 事件消息处理器对象
        /// </summary>
        public IReadOnlyDictionary<Type, object> HandlerObjects => new ReadOnlyDictionary<Type, object>(handlerObjects);

        /// <summary>
        /// 发布事件消息
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public async Task PublishAsync<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = options.Serialize(message),
            };
            try
            {
                await eventStore.AddAsync(@event);
                logger.LogDebug($"PublishAsync done.Message:{@event.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"PublishAsync error.{ex.GetType().Name}({ex.Message}).Message:{@event.Message}");
            }
        }

        /// <summary>
        /// 发布事件消息
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public void Publish<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = options.Serialize(message),
            };
            try
            {
                eventStore.Add(@event);
                logger.LogDebug($"Publish done.Message:{@event.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Publish error.{ex.GetType().Name}({ex.Message}).Message:{@event.Message}");
            }
        }

        /// <summary>
        /// 事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task SubscribeAsync<TEventMessage, TEventHandler>(TEventHandler handler = default)
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            try
            {
                await processStore.AddAsync(new FakeMQProcess
                {
                    Subscription = subscriptionId,
                });
                if (!subscriptions.ContainsKey(handlerType))
                {
                    subscriptions.Add(handlerType, messageType);
                    if (handler != null)
                        handlerObjects.Add(handlerType, handler);
                }
                logger.LogDebug($"SubscribeAsync done.Subscription:{subscriptionId}");
            }
            catch (Exception ex)
            {
                logger.LogError($"SubscribeAsync error.{ex.GetType().Name}({ex.Message}).Subscription:{subscriptionId}");
            }
        }

        /// <summary>
        /// 事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TEventMessage, TEventHandler>(TEventHandler handler = default)
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            try
            {
                processStore.Add(new FakeMQProcess
                {
                    Subscription = subscriptionId,
                });
                if (!subscriptions.ContainsKey(handlerType))
                {
                    subscriptions.Add(handlerType, messageType);
                    if (handler != null)
                        handlerObjects.Add(handlerType, handler);
                }
                logger.LogDebug($"Subscribe done.Subscription:{subscriptionId}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Subscribe error.{ex.GetType().Name}({ex.Message}).Subscription:{subscriptionId}");
            }
        }

        /// <summary>
        /// 取消事件消息订阅
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

                try
                {
                    await processStore.DeleteAsync(subscriptionId);
                    logger.LogDebug($"UnsubscribeAsync done.Subscription:{subscriptionId}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"UnsubscribeAsync error.{ex.GetType().Name}({ex.Message}).Subscription:{subscriptionId}");
                }
            }
        }

        /// <summary>
        /// 取消事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Unsubscribe<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);
            var subscriptionId = GetSubscriptionId(messageType, handlerType);

            if (subscriptions.ContainsKey(handlerType))
            {
                subscriptions.Remove(handlerType);

                try
                {
                    processStore.Delete(subscriptionId);
                    logger.LogDebug($"Unsubscribe done.Subscription:{subscriptionId}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Unsubscribe error.{ex.GetType().Name}({ex.Message}).Subscription:{subscriptionId}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHandling { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task HandleAsync()
        {
            if (IsHandling)
                return;

            IsHandling = true;
            logger.LogTrace("Event handling start");

            foreach (var item in subscriptions)
            {
                var messageType = item.Value;
                var handlerType = item.Key;

                var events = await GetEventsAsync(messageType, handlerType);
                if (events.IsNullOrEmpty())
                    continue;

                var handler = CreateHandler(handlerType);
                if (handler == null)
                {
                    logger.LogWarning($"Can't get handler.HandlerType:{handlerType.FullName}");
                    continue;
                }

                var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                foreach (var @event in events)
                {
                    logger.LogDebug($"Handling starts.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                    try
                    {
                        var result = await (Task<bool>)method.Invoke(handler, new[] { options.Deserialize(@event.Message, messageType) });

                        logger.LogDebug($"Handling result:{result}");

                        if (result)
                            await UpdateProcessTimeAsync(GetSubscriptionId(messageType, handlerType), @event.CreateTime);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Handling error:{ex.GetType().Name}({ex.Message})");
                        break;
                    }

                    await Task.Delay(50);
                }
            }

            IsHandling = false;
            logger.LogTrace("Event handling complte");
        }

        async Task<IEnumerable<FakeMQEvent>> GetEventsAsync(Type messageType, Type handlerType)
        {
            var subscriptionId = GetSubscriptionId(messageType, handlerType);
            try
            {
                var fromTime = await GetProcessTimeAsync(subscriptionId);
                var toTime = DateTimeOffset.Now;

                return await eventStore.GetAsync(messageType.Name, fromTime, toTime);
            }
            catch (Exception ex)
            {
                logger.LogError($"Events getting error:{ex.GetType().Name}({ex.Message})");
                return null;
            }
        }
        object CreateHandler(Type handlerType)
        {
            var handler = handlerObjects.ContainsKey(handlerType) ? handlerObjects[handlerType] : null;
            if (handler == null)
            {
#if NET45
                handler = Activator.CreateInstance(handlerType);
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
                handler = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, handlerType);
#endif
            }
            return handler;
        }
        async Task<DateTimeOffset> GetProcessTimeAsync(string subscriptionId)
        {
            if (!processTimes.ContainsKey(subscriptionId))
                processTimes[subscriptionId] = (await processStore.GetAsync(subscriptionId)).LastHandleTime;

            return processTimes[subscriptionId];
        }
        async Task UpdateProcessTimeAsync(string subscriptionId, DateTimeOffset handleTime)
        {
            processTimes[subscriptionId] = handleTime;
            try
            {
                logger.LogDebug($"Process updating starts.Handle time:{handleTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz")}");
                await processStore.UpdateAsync(subscriptionId, handleTime);
                logger.LogDebug($"Process updating done");
            }
            catch (Exception ex)
            {
                logger.LogError($"Process updating error:{ex.GetType().Name}({ex.Message})");
            }
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.Name}>{handlerType.FullName}";
    }
}
