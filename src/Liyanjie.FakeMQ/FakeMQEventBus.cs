using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeMQEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
#if NET45
            this.options = serviceProvider.GetService(typeof(FakeMQOptions)) as FakeMQOptions;
#endif
#if NETSATNDARD2_0||NETSTANDARD2_1
            this.options = serviceProvider.GetRequiredService<IOptions<FakeMQOptions>>().Value;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FakeMQEventBus(FakeMQOptions options)
        {
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
                options.Log("Information", $"事件发布成功。事件消息：{@event.Message}");
            }
            catch (Exception ex)
            {
                options.LogError(ex, $"事件发布异常。事件消息：{@event.Message}");
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
                options.Log("Information", $"事件发布成功。事件消息：{@event.Message}");
            }
            catch (Exception ex)
            {
                options.LogError(ex, $"事件发布异常。事件消息：{@event.Message}");
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
                options.Log("Information", $"添加事件订阅。订阅ID：{subscriptionId}");
            }
            catch (Exception ex)
            {
                options.LogError(ex, $"事件订阅异常。订阅ID：{subscriptionId}");
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
                options.Log("Information", $"添加事件订阅。订阅ID：{subscriptionId}");
            }
            catch (Exception ex)
            {
                options.LogError(ex, $"事件订阅异常。订阅ID：{subscriptionId}");
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
                    options.Log("Information", $"取消事件订阅。订阅ID：{subscriptionId}");
                }
                catch (Exception ex)
                {
                    options.LogError(ex, $"取消订阅异常。订阅ID：{subscriptionId}");
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
                    options.Log("Information", $"取消事件订阅。订阅ID：{subscriptionId}");
                }
                catch (Exception ex)
                {
                    options.LogError(ex, $"取消订阅异常。订阅ID：{subscriptionId}");
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
            options.Log("Trace", "Event handling start.");

            var endTimestamp = DateTimeOffset.Now.Ticks;
            using var processStore = options.GetProcessStore(serviceProvider);
            using var eventStore = options.GetEventStore(serviceProvider);
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
                        options.Log("Warning", $"Can not create instance of handlerType:{handlerType.FullName}.");
                        continue;
                    }

                    var concreteType = typeof(IFakeMQEventHandler<>).MakeGenericType(messageType);
                    var method = concreteType.GetTypeInfo().GetMethod(nameof(IFakeMQEventHandler<object>.HandleAsync));
                    foreach (var @event in events)
                    {
                        options.Log("Debug", $"Handle begins.(handlerType:{handlerType.FullName},messageType:{messageType.FullName},message:{@event.Message})");

                        var result = await (Task<bool>)method.Invoke(handler, new[] { options.Deserialize(@event.Message, messageType) });

                        options.Log("Debug", $"Handle result:{result}.");

                        if (result)
                        {
                            await processStore.UpdateAsync(subscriptionId, @event.Timestamp);
                            options.Log("Debug", $"Update process:{@event.Timestamp}");
                        }

                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    options.LogError(ex, $"Handling error:{ex.Message}.(handlerType:{handlerType.FullName},messageType:{messageType.FullName})");
                }
            }

            IsHandling = false;
            options.Log("Trace", "Event handling complte.");
        }

        static string GetSubscriptionId(Type messageType, Type handlerType) => $"{messageType.Name}>{handlerType.FullName}";
        static object GetServiceOrCreateInstance(IServiceProvider serviceProvider, Type serviceType)
        {
#if NET45
            return serviceProvider.GetServiceOrCreateInstance(serviceType);
#endif
#if NETSATNDARD2_0||NETSTANDARD2_1
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider.CreateScope().ServiceProvider, serviceType);
#endif
            throw new NotImplementedException();
        }
    }
}
