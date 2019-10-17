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
        bool Add(FakeMQEvent @event);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        FakeMQEvent Get(string type, long timestamp);
    }
}
