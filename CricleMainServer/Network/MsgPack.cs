using CricleMainServer.Network.Configuration;
using CricleMainServer.Network.NetInterface;
using CricleMainServer.Tools;
using System;

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
    /// socket用于收发Tcp消息包类
    /// </summary>
    class MsgPack : IMsgPack
    {
        private byte msgStartTag;
        private int msgSize;
        private MsgBasic msgData;
        private byte msgEndTag;

        public byte MsgStartTag { get => msgStartTag; set => msgStartTag = value; }
        public int MsgSize { get => msgSize; set => msgSize = value; }
        internal MsgBasic MsgData { get => msgData; set => msgData = value; }
        public byte MsgEndTag { get => msgEndTag; set => msgEndTag = value; }

        public MsgPack(){}

        #region 打包数据

        /// <summary>
        /// 仅供测试使用打包数据
        /// </summary>
        /// <param name="msgStartTag"></param>
        /// <param name="msgSize"></param>
        /// <param name="msgData"></param>
        /// <param name="msgEndTag"></param>
        public MsgPack(byte msgStartTag, MsgBasic msgData, byte msgEndTag)
        {
            this.msgStartTag = msgStartTag; this.msgData = msgData; this.msgSize = this.msgData.GetMsgBasicSize(); this.msgEndTag = msgEndTag;
        }

        /// <summary>
        /// 实际使用打包数据
        /// </summary>
        /// <param name="msgData">基础消息包</param>
        public MsgPack(MsgBasic msgData)
        {
            this.msgStartTag = MsgConfiguration.MSG_START_TAG; this.msgEndTag = MsgConfiguration.MSG_END_TAG;
            this.msgData = msgData;this.msgSize = this.msgData.GetMsgBasicSize();
        }

        public byte[] GetMsgPackData()
        {
            return ConvertTypeTool.MergerByteArrayForPackMessage(BitConverter.GetBytes(msgSize), msgData.GetMsgBasicData(), msgStartTag, msgEndTag);
        }

        #endregion

        #region 解包数据

        /// <summary>
        /// 解包函数,测试用
        /// </summary>
        /// <param name="msgPackData">一条完整的消息包</param>
        public MsgPack(byte[] msgPackData)
        {
            //获取消息开头和结尾标识符
            this.msgStartTag = msgPackData[0];
            this.msgEndTag = msgPackData[msgPackData.Length - 1];

            //获取消息大小
            byte[] sizeBytes = new byte[4];
            Buffer.BlockCopy(msgPackData, 1, sizeBytes, 0, 4);
            this.msgSize = BitConverter.ToInt32(sizeBytes, 0);

            //获取主体消息
            byte[] basicBytes = new byte[msgPackData.Length - 6];
            Buffer.BlockCopy(msgPackData, 5, basicBytes, 0, basicBytes.Length);
            this.msgData = new MsgBasic(basicBytes);
        }

        /// <summary>
        /// 解包函数，实际使用
        /// </summary>
        /// <param name="msgSize"></param>
        /// <param name="msgBasicData"></param>
        public MsgPack(int msgSize,byte[] msgBasicData)
        {
            this.msgStartTag = MsgConfiguration.MSG_START_TAG;this.msgEndTag = MsgConfiguration.MSG_END_TAG;
            this.msgSize = msgSize;this.msgData = new MsgBasic(msgBasicData);
        }

        // 在socket异步接收中，我将接收到的数据存入一条byte数组中，供同时读取和写入
        // 为使读写不冲突，设立两个变量指标readIndex与writeIndex，readIndex表示解包数据,并在写入读取时有以下情况存在：
        //  一、在写入数据时(接收数据时),socket将从write位置开始异步写入:
        // 1.当writeIndex<readIndex-1,写入到readIndex前一位停止。
        // 2.当writeIndex>=readIndex,可写入到数组的最后一位。
        // 3.当writeIndex=readIndex-1,进入等待线程池
        // 4.当writeindex=数组大小,writeIndex=0;
        // 二、在读取数据是(解包数据时),该函数将从read位置开始读取:
        // 1.当readIndex<writeIndex,可读取数据到writeIndex前一位。
        // 2.当readIndex>writeIndex,可读取数据到数组最后一位并归0，再按1规则继续读取
        // 3.当readIndex=writeIndex,此时不可读取,表示没有新数据存入
        // 但在实际应用时还可以考虑到一个消息包至少的大小(在msgData=null时)为1(startTag)+4(size)+4(type)+8(time)+4(flag)+1(endTag)=22字节,故可优化读取规则：
        // 1.当readIndex<writeIndex,判断writeIndex-readIndex>22?可读取数据段为readIndex至writeIndex-1:不可读取，消息不完整。
        // 2.当readIndex>writeIndex,判断length-readIndex+writeIndex>22?可读取数据段为readIndex至length-1加上0至writeIndex-1:不可读取，消息不完整
        // 3.当readIndex=writeIndex,此时不可读取,表示没有新数据存入
        /// 解包步骤:在socket异步接收消息时，并非每次都接收到的不一定是一条完整的消息包，他可能小于一条或大于一条
        /// 所以在解包时需要分步判断：
        /// 1.判断开头是否为开始标志符，若不是可能消息已被篡改，断开重连此链接
        /// 2.判断开始标识符后4字节转换后的数字是否正常(表示消息包大小，消息包大小不会超过规定大小)，若不是可能消息已被篡改，断开重连此链接
        /// 3.判断接收到的数据是否比数据包大小大，若不是表示消息未接收完全，再次开始异步接收
        /// 4.判断结束标识符是否一致
        // 如何解包？
        // 1.将可读取的数据全部取出到一个新数组再解析(浪费内存)
        // 2.判断开始标识符，数据大小、结束标识符后再将一个完整消息包取出再解析(可以设置静态方法使用此判断后再将该完整消息包传递给解包函数)[采用此方法]
        // 3.判断开始标识符，数据大小、结束标识符时也会将消息记录到类中(边判断边读取)
        /// -2021.3.16
        /// 读写规则修改
        // 在socket异步接收中，将接收到的数据存入一条byte数组中，此数组可供同时读取和写入
        // 为使读写不冲突，需设立两个变量指标readIndex与writeIndex，readIndex表示解包数据,并在写入读取时有以下情况存在：
        //  一、在写入数据时(接收数据时),socket将从write位置开始异步写入:
        // 1.当writeIndex<readIndex-1,写入到readIndex前一位停止,写入大小:readIndex-writeIndex-1。(留一字节空白)
        // 2.当writeIndex>=readIndex,若readIndex>0，则可写入buffSize-writeIndex;若readIndex==0;则只可写入buffSize-writeIndex-1。
        // 3.当writeIndex=readIndex-1,进入等待线程池
        // 4.当writeindex=数组大小,writeIndex=0;
        // 二、在读取数据是(解包数据时),该函数将从read位置开始读取:
        // 1.当readIndex<writeIndex,判断writeIndex-readIndex>22?可读取数据段为readIndex至writeIndex-1:不可读取，消息不完整。
        // 2.当readIndex>writeIndex,判断length-readIndex+writeIndex>22?可读取数据段为readIndex至length-1加上0至writeIndex-1:不可读取，消息不完整
        // 3.当readIndex=writeIndex,此时不可读取,表示没有新数据存入
        ///-2021.3.26
        /// <summary>
        /// 查验receivedData中的数据
        /// </summary>
        /// <param name="receivedData">接收数据缓存数组</param>
        /// <param name="readIndex">读取数据起始位置</param>
        /// <param name="writeIndex">写入数据起始位置</param>
        /// <param name="state">返回检查状态：1.成功;2.不完整;3:消息错乱,需要重新连接客户端</param>
        /// <returns>返回检查到的第一个消息包</returns>
        public static MsgPack CheckMsgInfo(ref byte[] receivedData, ref int readIndex,ref int writeIndex,out int state)
        {
            if (readIndex < writeIndex)
            {
                if (writeIndex - readIndex > 22)
                {
                    //判断首字节是否与起始标识符一致
                    if (receivedData[readIndex] == MsgConfiguration.MSG_START_TAG)
                    {
                        byte[] sizeBytes = new byte[4];
                        Buffer.BlockCopy(receivedData, readIndex + 1, sizeBytes, 0, 4);
                        int msgSize = BitConverter.ToInt32(sizeBytes, 0);
                        //判断剩余可读数据中是否存在可解析消息包
                        if (writeIndex - readIndex > 5 + msgSize)
                        {
                            if (receivedData[readIndex + msgSize + 5] == MsgConfiguration.MSG_END_TAG)
                            {
                                byte[] msgBasicData = new byte[msgSize];
                                Buffer.BlockCopy(receivedData, readIndex + 5, msgBasicData, 0, msgSize);
                                MsgPack msgPack = new MsgPack(msgSize, msgBasicData);
                                readIndex = readIndex + 6 + msgSize;
                                state = 1;
                                return msgPack;
                            }
                            else
                            {
                                state = 3;
                                return null;
                            }
                        }
                    }
                    else
                    {
                        state = 3;
                        return null;
                    }
                }
            }
            else if (readIndex > writeIndex)
            {
                if (MsgConfiguration.MSG_BUFF_SIZE - readIndex + writeIndex > 22)
                {
                    //判断首字节是否与起始标识符一致
                    if (receivedData[readIndex] == MsgConfiguration.MSG_START_TAG)
                    {
                        int msgBasicIndex;
                        byte[] sizeBytes = ConvertTypeTool.LoopReadFromArray(ref receivedData, MsgConfiguration.MSG_BUFF_SIZE, (readIndex + 1), 4, out msgBasicIndex);
                        int msgSize = BitConverter.ToInt32(sizeBytes, 0);
                        if (MsgConfiguration.MSG_BUFF_SIZE - readIndex + writeIndex > 5 + msgSize)
                        {
                            int endTagIndex;
                            if (msgBasicIndex > readIndex)
                            {
                                int offset = MsgConfiguration.MSG_BUFF_SIZE - msgBasicIndex;
                                if (offset >= 1 + msgSize)
                                {
                                    endTagIndex = readIndex + 5 + msgSize;
                                }
                                else
                                {
                                    endTagIndex = msgSize - offset;
                                }
                            }
                            else
                            {
                                endTagIndex = msgBasicIndex + msgSize;
                            }
                            if (receivedData[endTagIndex] == MsgConfiguration.MSG_END_TAG)
                            {
                                byte[] msgPackData = ConvertTypeTool.LoopReadFromArray(ref receivedData, MsgConfiguration.MSG_BUFF_SIZE, msgBasicIndex, msgSize, out readIndex);

                                if (readIndex != MsgConfiguration.MSG_BUFF_SIZE - 1)
                                {
                                    readIndex++;
                                }
                                else
                                {
                                    readIndex = 0;
                                }

                                MsgPack msgPack = new MsgPack(msgSize, msgPackData);
                                state = 1;
                                return msgPack;
                            }
                            else
                            {
                                state = 3;
                                return null;
                            }
                        }
                    }
                    else
                    {
                        state = 3;
                        return null;
                    }
                }
            }
            else if (readIndex == writeIndex)
            {
                state = 0;
                return null;
            }
            state = 2;
            return null;
        }
        #endregion
    }
}
