using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NET45
using NLog;
#else
using Microsoft.Extensions.DependencyInjection;
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
        }

        readonly Func<IFakeMQEventStore> getEventStore;
        readonly Func<IFakeMQProcessStore> getProcessStore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getEventStore"></param>
        /// <param name="getProcessStore"></param>
        public FakeMQEventBus(
            Func<IFakeMQEventStore> getEventStore,
            Func<IFakeMQProcessStore> getProcessStore)
        {
            this.getEventStore = getEventStore;
            this.getProcessStore = getProcessStore;
        }

        IFakeMQEventStore EventStore => serviceProvider?.GetService(typeof(IFakeMQEventStore)) as IFakeMQEventStore ?? getEventStore();
        IFakeMQProcessStore ProcessStore => serviceProvider?.GetService(typeof(IFakeMQProcessStore)) as IFakeMQProcessStore ?? getProcessStore();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public async Task PublishAsync<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = FakeMQEvent.GetMsgString(message),
            };
            await TryExecuteAsync(async () => await EventStore.AddAsync(@event));
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

            if (await ProcessStore.AddAsync(new FakeMQProcess
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
                await TryExecuteAsync(async () => await ProcessStore.DeleteAsync(subscriptionId));
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
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    try
                    {
                        var timestamp = long.MaxValue;
                        var _processStore = ProcessStore;
                        foreach (var item in subscriptions.Select(_ => GetSubscriptionId(_.Value, _.Key)))
                        {
                            timestamp = Math.Min(timestamp, (await _processStore.GetAsync(item)).Timestamp);
                        }
                        await EventStore.ClearAsync(timestamp);
                        LogInformation($"Cleare event store at timestamp:{timestamp}.");
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Error when clear event store.");
                    }
                }
            });
            var tasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                LogTrace($"Handle event loop start.");

                tasks.Clear();
                foreach (var item in subscriptions)
                {
                    var messageType = item.Value;
                    var handlerType = item.Key;
                    var subscriptionId = GetSubscriptionId(messageType, handlerType);

                    var timestamp = (await ProcessStore.GetAsync(subscriptionId)).Timestamp;

                    var @event = await EventStore.GetAsync(messageType.Name, timestamp);
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

                    tasks.Add(Task.Run(async () =>
                    {
                        var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                        var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                        var result = await (Task<bool>)method.Invoke(handler, new[] { @event.GetMsgObject(messageType) });
                      
                        LogTrace($"Handle result:{result}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                        if (result)
                            await TryExecuteAsync(async () => await ProcessStore.UpdateAsync(subscriptionId, @event.Timestamp));
                    }));
                }

                await Task.WhenAll(tasks);

                LogTrace($"Handle event loop complte.");

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            LogInformation($"FakeMQ process stop.");
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

        object GetServiceOrCreateInstance(IServiceProvider serviceProvider, Type serviceType)
        {
#if NET45
            return serviceProvider.GetServiceOrCreateInstance(serviceType);
#else
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, serviceType);
#endif
        }
        void LogTrace(string message)
        {
#if NET45
            logger.Trace(message);
#else
            logger.LogTrace(message);
#endif
        }
        void LogInformation(string message)
        {
#if NET45
            logger.Info(message);
#else
            logger.LogInformation(message);
#endif
        }
        void LogWarning(string message)
        {
#if NET45
            logger.Warn(message);
#else
            logger.LogWarning(message);
#endif
        }
        void LogError(Exception exception, string message)
        {
#if NET45
            logger.Error(exception, message);
#else
            logger.LogError(exception, message);
#endif
        }
    }
}
