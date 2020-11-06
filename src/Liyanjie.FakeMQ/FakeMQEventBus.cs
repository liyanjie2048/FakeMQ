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
        readonly IDictionary<Type, Type> eventHandlers = new Dictionary<Type, Type>();
        readonly IDictionary<Type, DateTimeOffset> processTimes = new Dictionary<Type, DateTimeOffset>();

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
        public IReadOnlyDictionary<Type, Type> EventHandlers => new ReadOnlyDictionary<Type, Type>(eventHandlers);

        /// <summary>
        /// 发布事件消息
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public async Task PublishEventAsync<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = options.Serialize(message),
            };
            try
            {
                await eventStore.AddAsync(@event);
                logger.LogDebug($"PublishEventAsync done.Message:{@event.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"PublishEventAsync error.{ex.GetType().Name}({ex.Message}).Message:{@event.Message}");
            }
        }

        /// <summary>
        /// 发布事件消息
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <param name="message"></param>
        public void PublishEvent<TEventMessage>(TEventMessage message)
        {
            var @event = new FakeMQEvent
            {
                Type = typeof(TEventMessage).Name,
                Message = options.Serialize(message),
            };
            try
            {
                eventStore.Add(@event);
                logger.LogDebug($"PublishEvent done.Message:{@event.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"PublishEvent error.{ex.GetType().Name}({ex.Message}).Message:{@event.Message}");
            }
        }

        /// <summary>
        /// 事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task RegisterEventHandlerAsync<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);

            try
            {
                await processStore.AddAsync(new FakeMQProcess
                {
                    HandlerType = handlerType.FullName,
                });
                if (!eventHandlers.ContainsKey(handlerType))
                {
                    eventHandlers.Add(handlerType, messageType);
                }
                logger.LogDebug($"RegisterEventHandlerAsync done.EventHandler:{GetEventHandlerId(messageType, handlerType)}");
            }
            catch (Exception ex)
            {
                logger.LogError($"RegisterEventHandlerAsync error.{ex.GetType().Name}({ex.Message}).EventHandler:{GetEventHandlerId(messageType, handlerType)}");
            }
        }

        /// <summary>
        /// 事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void RegisterEventHandler<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);

            try
            {
                processStore.Add(new FakeMQProcess
                {
                    HandlerType = handlerType.FullName,
                });
                if (!eventHandlers.ContainsKey(handlerType))
                {
                    eventHandlers.Add(handlerType, messageType);
                }
                logger.LogDebug($"RegisterEventHandler done.EventHandler:{GetEventHandlerId(messageType, handlerType)}");
            }
            catch (Exception ex)
            {
                logger.LogError($"RegisterEventHandler error.{ex.GetType().Name}({ex.Message}).EventHandler:{GetEventHandlerId(messageType, handlerType)}");
            }
        }

        /// <summary>
        /// 取消事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public async Task UnRegisterEventHandlerAsync<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);

            if (eventHandlers.ContainsKey(handlerType))
            {
                eventHandlers.Remove(handlerType);

                try
                {
                    await processStore.DeleteAsync(handlerType.FullName);
                    logger.LogDebug($"UnRegisterEventHandlerAsync done.EventHandler:{GetEventHandlerId(messageType, handlerType)}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"UnRegisterEventHandlerAsync error.{ex.GetType().Name}({ex.Message}).EventHandler:{GetEventHandlerId(messageType, handlerType)}");
                }
            }
        }

        /// <summary>
        /// 取消事件消息订阅
        /// </summary>
        /// <typeparam name="TEventMessage"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void UnRegisterEventHandler<TEventMessage, TEventHandler>()
            where TEventHandler : IFakeMQEventHandler<TEventMessage>
        {
            var messageType = typeof(TEventMessage);
            var handlerType = typeof(TEventHandler);

            if (eventHandlers.ContainsKey(handlerType))
            {
                eventHandlers.Remove(handlerType);

                try
                {
                    processStore.Delete(handlerType.FullName);
                    logger.LogDebug($"UnRegisterEventHandler done.EventHandler:{GetEventHandlerId(messageType, handlerType)}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"UnRegisterEventHandler error.{ex.GetType().Name}({ex.Message}).EventHandler:{GetEventHandlerId(messageType, handlerType)}");
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
            logger.LogTrace("HandleAsync start");

            foreach (var item in eventHandlers)
            {
                var messageType = item.Value;
                var handlerType = item.Key;

                var events = await GetEventsAsync(messageType, handlerType);
                if (events.IsNullOrEmpty())
                    continue;

                var handler = CreateHandler(handlerType);
                if (handler == null)
                {
                    logger.LogWarning($"Can not get handler.HandlerType:{handlerType.FullName}");
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
                            await UpdateProcessTimeAsync(handlerType, @event.CreateTime);
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
            logger.LogTrace("HandleAsync done");
        }

        async Task<IEnumerable<FakeMQEvent>> GetEventsAsync(Type messageType, Type handlerType)
        {
            try
            {
                var fromTime = await GetProcessTimeAsync(handlerType);
                var toTime = DateTimeOffset.Now;

                return await eventStore.GetAsync(messageType.Name, fromTime, toTime);
            }
            catch (Exception ex)
            {
                logger.LogError($"GetEventsAsync error:{ex.GetType().Name}({ex.Message})");
                return null;
            }
        }
        object CreateHandler(Type handlerType)
        {
            var handler =
#if NET45
                Activator.CreateInstance(handlerType);
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
                ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, handlerType);
#endif
            return handler;
        }
        async Task<DateTimeOffset> GetProcessTimeAsync(Type handlerType)
        {
            if (!processTimes.ContainsKey(handlerType))
                processTimes[handlerType] = (await processStore.GetAsync(handlerType.FullName)).LastHandleTime;

            return processTimes[handlerType];
        }
        async Task UpdateProcessTimeAsync(Type handlerType, DateTimeOffset handleTime)
        {
            processTimes[handlerType] = handleTime;
            try
            {
                logger.LogDebug($"UpdateProcessTimeAsync start.Handle time:{handleTime:yyyy-MM-dd HH:mm:ss.fffffff zzz}");
                await processStore.UpdateAsync(handlerType.FullName, handleTime);
                logger.LogDebug($"UpdateProcessTimeAsync done");
            }
            catch (Exception ex)
            {
                logger.LogError($"UpdateProcessTimeAsync error:{ex.GetType().Name}({ex.Message})");
            }
        }

        static string GetEventHandlerId(Type messageType, Type handlerType) => $"[{messageType.Name}]->[{handlerType.FullName}]";
    }
}
