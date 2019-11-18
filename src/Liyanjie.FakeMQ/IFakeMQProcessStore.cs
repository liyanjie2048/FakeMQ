using System;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFakeMQProcessStore : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        Task<bool> AddAsync(FakeMQProcess process);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task<FakeMQProcess> GetAsync(string subscription);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(string subscription, long timestamp);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string subscription);
    }
}
