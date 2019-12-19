using System;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFakeMQProcessStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        Task AddAsync(FakeMQProcess process);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        void Add(FakeMQProcess process);

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
        /// <returns></returns>
        FakeMQProcess Get(string subscription);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="handleTime"></param>
        /// <returns></returns>
        Task UpdateAsync(string subscription, DateTimeOffset handleTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="handleTime"></param>
        void Update(string subscription, DateTimeOffset handleTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task DeleteAsync(string subscription);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        void Delete(string subscription);
    }
}
