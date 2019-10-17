using System.Collections.Generic;

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
        bool Add(FakeMQProcess process);

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
        /// <param name="timestamp"></param>
        /// <returns></returns>
        bool Update(string subscription, long timestamp);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        bool Delete(string subscription);
    }
}
