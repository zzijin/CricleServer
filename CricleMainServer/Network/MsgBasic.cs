using CricleMainServer.Network.NetInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CricleMainServer.Tools;

namespace CricleMainServer.Network
{
    ///*************************************************** 消息包格式
    ///     是否使用              名称                  变量名              类型         长度(字节)            说明
    ///        Y              消息包开始标识符        msgStartTag           byte             1                 包头标识，固定值
    ///        Y              消息包大小              msgSize               int32            4                 该消息包长度,MainMsg总长度
    ////////////MainMsg包含:        
    ///        Y              操作号/消息类别         msgType               int32            4                 该消息包操作号，用于对相应消息包进行处理
    ///        Y              消息主体/数据           msgData               byte[]         不定长              该消息包的主体数据
    ///        Y              时间戳                  msgTime               long             8                 该消息包发出时的时间戳
    ///        Y              消息数据加密标志        msgFlag               int32            4                 该消息包主体数据的加密方式
    ////////////以上
    ///        Y              消息包结束标识符        msgEndTag             byte             1                 包尾标识符
    ///*************************************************** 消息包格式
    ///-2021.3.14
    /// <summary>
    /// socket收发TCP主体消息类
    /// </summary>
    class MsgBasic : IMsgBasic
    {
        private int msgType;
        private byte[] msgData;
        private long msgTime;
        private int msgFlag;

        public int MsgType { get => msgType; set => msgType = value; }
        public byte[] MsgData { get => msgData; set => msgData = value; }
        public long MsgTime { get => msgTime; set => msgTime = value; }
        public int MsgFlag { get => msgFlag; set => msgFlag = value; }
        public MsgBasic() { }

        //////////////////////////////////打包数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgType">消息类别</param>
        /// <param name="msgData">消息数据</param>
        /// <param name="msgTime">消息时间戳</param>
        /// <param name="msgFlag">消息加密标志</param>
        public MsgBasic(int msgType, byte[] msgData,long msgTime,int msgFlag)
        {
            this.msgType = msgType;this.msgData = msgData;this.msgTime = msgTime;this.msgFlag = msgFlag;
        }

        /// <summary>
        /// 获取基础消息的byte数组形式
        /// </summary>
        /// <returns></returns>
        public byte[] GetMsgBasicData()
        {

            byte[] bytes1 = ConvertTypeTool.MergerByteArray(BitConverter.GetBytes(MsgType), msgData);
            byte[] bytes2 = ConvertTypeTool.MergerByteArray(bytes1, BitConverter.GetBytes(msgTime));
            byte[] bytes3 = ConvertTypeTool.MergerByteArray(bytes2, BitConverter.GetBytes(msgFlag));
            return bytes3;
        }

        /// <summary>
        /// 获取基础消息大小
        /// </summary>
        /// <returns></returns>
        public int GetMsgBasicSize()
        {
            return 16 + msgData.Length;
        }
        //////////////////////////////////打包数据

        //////////////////////////////////解包数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicMsgData">基础消息的byte数组形式</param>
        public MsgBasic(byte[] basicMsgData)
        {
            //取出前四位字节，这是消息类型
            byte[] typeBytes = new byte[4];
            Buffer.BlockCopy(basicMsgData, 0, typeBytes, 0, 4);
            this.msgType = BitConverter.ToInt32(typeBytes, 0);

            //取出前4到后12之间的字节，这是主体数据
            int msgDataCount = basicMsgData.Count() - 16;
            this.msgData = new byte[msgDataCount];
            Buffer.BlockCopy(basicMsgData, 4, msgData, 0, msgDataCount);

            //取出后12-后4的字节，这是时间戳
            int msgTimeIndex = msgDataCount + 4;
            byte[] timeBytes = new byte[8];
            Buffer.BlockCopy(basicMsgData, msgTimeIndex, timeBytes, 0, 8);
            this.msgTime = BitConverter.ToInt64(timeBytes, 0);

            //取出后4的字节，这是加密标志
            int msgFlagIndex = msgTimeIndex + 8;
            byte[] flagBytes = new byte[4];
            Buffer.BlockCopy(basicMsgData, msgFlagIndex, flagBytes, 0, 4);
            this.msgFlag = BitConverter.ToInt32(flagBytes, 0);
        }
        //////////////////////////////////解包数据
        
        
    }
}
