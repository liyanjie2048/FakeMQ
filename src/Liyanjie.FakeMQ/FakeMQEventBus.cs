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

using Polly;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FakeMQEventBus(
#if !NET45
            Func<ILogger<FakeMQEventBus>> getLogger,
#endif
            FakeMQOptions options)
        {
#if NET45
            this.logger = LogManager.GetCurrentClassLogger();
#else
            this.logger = getLogger?.Invoke();
#endif
            this.options = options;
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
            await TryExecuteAsync(async () =>
            {
                using var eventStore = options.GetEventStore(serviceProvider);
                return await eventStore.AddAsync(@event);
            });
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

            using var processStore = options.GetProcessStore(serviceProvider);
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

                using var processStore = options.GetProcessStore(serviceProvider);
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
            LogInformation($"FakeMQ process start.");

            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(options.EventCleaningLoopTimeSpan);
                    LastEventCleaningLoopTime = DateTimeOffset.Now;

                    var timestamp = long.MaxValue;
                    using var processStore = options.GetProcessStore(serviceProvider);
                    foreach (var item in subscriptions.Select(_ => GetSubscriptionId(_.Value, _.Key)))
                    {
                        timestamp = Math.Min(timestamp, (await processStore.GetAsync(item)).Timestamp);
                    }

                    LogInformation($"Clean events at timestamp:{timestamp}.");

                    using var eventStore = options.GetEventStore(serviceProvider);
                    try
                    {
                        await eventStore.CleanAsync(timestamp);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Error when clean events.");
                    }
                }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                LogDebug($"Event handling loop start.");
                LastEventHandlingLoopTime = DateTimeOffset.Now;

                using var processStore = options.GetProcessStore(serviceProvider);
                using var eventStore = options.GetEventStore(serviceProvider);
                foreach (var item in subscriptions)
                {
                    var messageType = item.Value;
                    var handlerType = item.Key;
                    var subscriptionId = GetSubscriptionId(messageType, handlerType);

                    var timestamp = (await processStore.GetAsync(subscriptionId)).Timestamp;

                    var @event = await eventStore.GetAsync(messageType.Name, timestamp);
                    if (@event == null)
                        continue;

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

                    bool result = false;
                    try
                    {
                        LogDebug($"Event handling begins.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                        result = await (Task<bool>)method.Invoke(handler, new[] { options.Deserialize(@event.Message, messageType) });
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Event handling error:{ex.Message}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");
                    }

                    LogDebug($"Event handling result:{result}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                    if (result)
                        await TryExecuteAsync(async () => await processStore.UpdateAsync(subscriptionId, @event.Timestamp));
                }

                LogDebug($"Event handling loop complte.");

                await Task.Delay(options.EventHandlingLoopTimeSpan);
            }

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
        static async Task<bool> TryExecuteAsync(Func<Task<bool>> func, int retryCount = 3)
        {
            if (func != null)
                return await Policy
                    .HandleResult<Task<bool>>(task => task.Result == false)
                    .Retry(retryCount)
                    .Execute(func);

            return false;
        }
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
