using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEventMessage"></typeparam>
    public interface IFakeMQEventHandler<TEventMessage>
        where TEventMessage : IFakeMQEventMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<bool> HandleAsync(TEventMessage @event);
    }
}
