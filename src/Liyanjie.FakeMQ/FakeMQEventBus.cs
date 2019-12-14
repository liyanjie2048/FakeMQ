using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NET45
using NLog;
#else
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

#if NET45
        readonly Logger logger;
#else
        readonly Microsoft.Extensions.Logging.ILogger<FakeMQEventBus> logger;
#endif
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
            this.logger = serviceProvider.GetService<ILogger<FakeMQEventBus>>();
            this.options = serviceProvider.GetRequiredService<IOptions<FakeMQOptions>>().Value;
#endif
        }

        public FakeMQEventBus(
            FakeMQOptions options
#if !NET45
            ,ILogger<FakeMQEventBus> logger
#endif
            )
        {
            this.options = options;
#if NET45
            this.logger = LogManager.GetCurrentClassLogger();
#else
            this.logger = logger;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool Processing { get; private set; }

        /// <summary>
        /// 所有订阅
        /// </summary>
        public IReadOnlyDictionary<Type, Type> Subscriptions => new ReadOnlyDictionary<Type, Type>(subscriptions);

        /// <summary>
        /// 事件消息处理器对象
        /// </summary>
        public IReadOnlyDictionary<Type, object> HandlerObjects => new ReadOnlyDictionary<Type, object>(handlerObjects);

        /// <summary>
        /// 最后事件清理时间
        /// </summary>
        public DateTimeOffset LastEventCleaningLoopTime { get; private set; }

        /// <summary>
        /// 最后事件处理时间
        /// </summary>
        public DateTimeOffset LastEventHandlingLoopTime { get; private set; }

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
                using var eventStore = options.GetEventStore(serviceProvider);
                await eventStore.AddAsync(@event);
            }
            catch (Exception ex)
            {
                LogError(ex, $"事件发布异常。事件消息：{@event.Message}");
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
                using var eventStore = options.GetEventStore(serviceProvider);
                eventStore.Add(@event);
            }
            catch (Exception ex)
            {
                LogError(ex, $"事件发布异常。事件消息：{@event.Message}");
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
                using var processStore = options.GetProcessStore(serviceProvider);
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
            }
            catch (Exception ex)
            {
                LogError(ex, $"事件订阅异常。订阅ID：{subscriptionId}");
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
                using var processStore = options.GetProcessStore(serviceProvider);
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
            }
            catch (Exception ex)
            {
                LogError(ex, $"事件订阅异常。订阅ID：{subscriptionId}");
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
                    using var processStore = options.GetProcessStore(serviceProvider);
                    await processStore.DeleteAsync(subscriptionId);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"取消订阅异常。订阅ID：{subscriptionId}");
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
                    using var processStore = options.GetProcessStore(serviceProvider);
                    processStore.Delete(subscriptionId);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"取消订阅异常。订阅ID：{subscriptionId}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task ProcessAsync(CancellationToken stoppingToken)
        {
            if (Processing)
                return;

            LogInformation($"FakeMQ process start.");

            while (!stoppingToken.IsCancellationRequested)
            {
                Processing = true;

                LogDebug($"Event handling loop start.");
                LastEventHandlingLoopTime = DateTimeOffset.Now;

                var endTimestamp = DateTimeOffset.Now.Ticks;
                using var processStore = options.GetProcessStore(serviceProvider);
                using var eventStore = options.GetEventStore(serviceProvider);
                foreach (var item in subscriptions)
                {
                    LastEventHandlingLoopTime = DateTimeOffset.Now;

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
                            LogWarning($"Can not create instance of handlerType:{handlerType.FullName}.");
                            continue;
                        }

                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                        foreach (var @event in events)
                        {
                            LastEventHandlingLoopTime = DateTimeOffset.Now;

                            LogDebug($"Handle begins.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                            var result = await (Task<bool>)method.Invoke(handler, new[] { options.Deserialize(@event.Message, messageType) });

                            LogDebug($"Handle result:{result}.");

                            if (result)
                            {
                                await processStore.UpdateAsync(subscriptionId, @event.Timestamp);
                                LogDebug($"Update process:{@event.Timestamp}");
                            }

                            await Task.Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Handling error:{ex.Message}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName})");
                    }
                }

                LogDebug($"Event handling loop complte.");

                await Task.Delay(options.EventHandlingLoopTimeSpan);
            }

            Processing = false;
            LogInformation($"FakeMQ process stop.");
        }

        void LogDebug(string message)
        {
#if NET45
            logger?.Debug(message);
#else
            logger?.LogDebug(message);
#endif
        }
        void LogInformation(string message)
        {
#if NET45
            logger?.Info(message);
#else
            logger?.LogInformation(message);
#endif
        }
        void LogWarning(string message)
        {
#if NET45
            logger?.Warn(message);
#else
            logger?.LogWarning(message);
#endif
        }
        void LogError(Exception exception, string message)
        {
#if NET45
            logger?.Error(exception, message);
#else
            logger?.LogError(exception, message);
#endif
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.Name}>{handlerType.FullName}";
        static object GetServiceOrCreateInstance(IServiceProvider serviceProvider, Type serviceType)
        {
#if NET45
            return serviceProvider.GetServiceOrCreateInstance(serviceType);
#else
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, serviceType);
#endif
        }
    }
}
