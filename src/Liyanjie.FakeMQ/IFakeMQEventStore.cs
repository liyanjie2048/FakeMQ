using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFakeMQEventStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<bool> AddAsync(FakeMQEvent @event);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        Task<FakeMQEvent> GetAsync(string type, long timestamp);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        Task ClearAsync(long timestamp);
    }
}
