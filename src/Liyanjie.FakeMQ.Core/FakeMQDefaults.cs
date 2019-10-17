using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQDefaults
    {
        /// <summary>
        /// 
        /// </summary>
        public static Func<object, string> JsonSerialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static Func<string, Type, object> JsonDeserialize { get; set; }
    }
}
