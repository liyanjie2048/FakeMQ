using System;

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
        void Add(FakeMQProcess process);

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
        void Update(string handlerType, DateTimeOffset handleTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType"></param>
        void Delete(string handlerType);
    }
}
