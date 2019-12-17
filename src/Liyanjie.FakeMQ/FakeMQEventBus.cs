using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        readonly IServiceProvider serviceProvider;
        readonly FakeMQOptions options;
        readonly FakeMQLogger logger;
        readonly IFakeMQEventStore eventStore;
        readonly IFakeMQProcessStore processStore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
#if NET45
            this.options = serviceProvider.GetService(typeof(FakeMQOptions)) as FakeMQOptions
                ?? throw new Exception($"No service fro type '{nameof(FakeMQOptions)}' has been registered");
            this.logger = serviceProvider.GetService(typeof(FakeMQLogger)) as FakeMQLogger
                ?? throw new Exception($"No service fro type '{nameof(FakeMQLogger)}' has been registered");
            this.eventStore = serviceProvider.GetService(typeof(IFakeMQEventStore)) as IFakeMQEventStore
                ?? throw new Exception($"No service fro type '{nameof(IFakeMQEventStore)}' has been registered");
            this.processStore = serviceProvider.GetService(typeof(IFakeMQProcessStore)) as IFakeMQProcessStore
                ?? throw new Exception($"No service fro type '{nameof(IFakeMQProcessStore)}' has been registered");
#endif
#if NETSATNDARD2_0||NETSTANDARD2_1
            this.options = serviceProvider.GetRequiredService<IOptions<FakeMQOptions>>().Value;
            this.logger = serviceProvider.GetRequiredService<FakeMQLogger>();
            this.eventStore = serviceProvider.GetRequiredService<IFakeMQEventStore>();
            this.processStore = serviceProvider.GetRequiredService<IFakeMQProcessStore>();
#endif
        }

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
                logger.LogDebug($"PublishAsync done.(message:{@event.Message})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PublishAsync error.(message:{@event.Message})");
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
                logger.LogDebug($"Publish done.(message:{@event.Message})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PublishMO error.(message:{@event.Message})");
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
                logger.LogDebug($"SubscribeAsync done.(subscriptionId:{subscriptionId})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"SubscribeAsync error.(subscriptionId:{subscriptionId})");
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
                logger.LogDebug($"Subscribe done.(subscriptionId:{subscriptionId})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Subscribe error.(subscriptionId:{subscriptionId})");
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
                    logger.LogDebug($"UnsubscribeAsync done.(subscriptionId:{subscriptionId})");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"UnsubscribeAsync error.(subscriptionId:{subscriptionId})");
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
                    logger.LogDebug($"Unsubscribe done.(subscriptionId:{subscriptionId})");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Unsubscribe error.(subscriptionId:{subscriptionId})");
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

            var endTimestamp = DateTimeOffset.Now.Ticks;
            foreach (var item in subscriptions)
            {
                var messageType = item.Value;
                var handlerType = item.Key;
                var subscriptionId = GetSubscriptionId(messageType, handlerType);

                try
                {
                    var startTimestamp = (await processStore.GetAsync(subscriptionId)).Timestamp;

                    var events = await eventStore.GetAsync(messageType.Name, startTimestamp, endTimestamp);
                    if (events.IsNullOrEmpty())
                    {
                        await processStore.UpdateAsync(subscriptionId, endTimestamp);
                        continue;
                    }

                    var handler = handlerObjects.ContainsKey(handlerType)
                        ? handlerObjects[handlerType]
                        : serviceProvider == null
                            ? Activator.CreateInstance(handlerType)
                            : GetServiceOrCreateInstance(serviceProvider, handlerType);

                    if (handler == null)
                    {
                        logger.LogWarning($"Can not get instance.HandlerType:{handlerType.FullName}");
                        continue;
                    }

                    var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                    var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                    foreach (var @event in events)
                    {
                        logger.LogDebug($"Handling begins.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                        var result = await (Task<bool>)method.Invoke(handler, new[] { options.Deserialize(@event.Message, messageType) });

                        logger.LogDebug($"Handling result:{result}");

                        if (result)
                        {
                            await processStore.UpdateAsync(GetSubscriptionId(messageType, handlerType), @event.Timestamp);
                            logger.LogDebug($"Handling process updated:{@event.Timestamp}");
                        }

                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Handling error:{ex.Message}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName})");
                }
            }

            IsHandling = false;
            logger.LogTrace("Event handling complte");
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.Name}>{handlerType.FullName}";
        static object GetServiceOrCreateInstance(IServiceProvider serviceProvider, Type serviceType)
        {
#if NET45
            return serviceProvider.GetServiceOrCreateInstance(serviceType);
#endif
#if NETSATNDARD2_0 || NETSTANDARD2_1
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, serviceType);
#endif
            throw new NotImplementedException();
        }
    }
}
