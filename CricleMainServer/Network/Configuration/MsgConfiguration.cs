namespace CricleMainServer.Network.Configuration
{
    class MsgConfiguration
    {
        /// <summary>
        /// 接收消息缓冲区大小
        /// </summary>
        public static readonly int MSG_BUFF_SIZE = 10 * 1024;
        /// <summary>
        /// 数据包开始标识符
        /// </summary>
        public static readonly byte MSG_START_TAG = 0x98;
        /// <summary>
        /// 数据包结束标识符
        /// </summary>
        public static readonly byte MSG_END_TAG = 0x99;
        /// <summary>
        /// 包裹最大大小
        /// </summary>
        public static readonly int PACK_SIZE = 1000;
    }
}
