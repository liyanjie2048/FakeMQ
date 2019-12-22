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
        /// <param name="handlerType"></param>
        /// <returns></returns>
        Task<FakeMQProcess> GetAsync(string handlerType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        FakeMQProcess Get(string handlerType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="handleTime"></param>
        /// <returns></returns>
        Task UpdateAsync(string handlerType, DateTimeOffset handleTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="handleTime"></param>
        void Update(string handlerType, DateTimeOffset handleTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        Task DeleteAsync(string handlerType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        void Delete(string handlerType);
    }
}
