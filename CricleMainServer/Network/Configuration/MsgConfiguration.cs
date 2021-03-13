using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network.Configuration
{
    class MsgConfiguration
    {
        /// <summary>
        /// 数据包开始标识符
        /// </summary>
        public static readonly byte DATA_START_TAG = 0x98;
        /// <summary>
        /// 数据包结束标识符
        /// </summary>
        public static readonly byte DATA_END_TAG = 0x99;
        /// <summary>
        /// 包裹最大大小
        /// </summary>
        public static readonly int PACK_SIZE = 1000;
    }
}
