using CricleMainServer.Network.Configuration;
using System;
using System.Net.Sockets;
using System.Threading;

namespace CricleMainServer.Network
{
    delegate void DSendMyClientData(MsgPack msgPack);
    class ClientConn
    {
        //该链接的基础(必要)信息
        /// <summary>
        /// 该链接在链接池中的位置
        /// </summary>
        private int clientIndex;
        /// <summary>
        /// 该链接使用状态
        /// </summary>
        private bool clientState;
        /// <summary>
        /// 该链接被使用的次数(因放在统计数据中)
        /// </summary>
        //private int clienUsedTimes;
        /// <summary>
        /// 该链接的连接客户端的套接字信息
        /// </summary>
        private Socket clientSocket;
        /// <summary>
        /// 该链接本次连接客户端的起始时间
        /// </summary>
        private long clientConnStartTime;
        private long receivePackNumber;

        //客户端需长时间保存的数据，如登录用户的UID，数据项多可以用单独的用户类保存
        /// <summary>
        /// 该链接连接的客户端的登录用户的UID
        /// </summary>
        private int clientUID;

        //客户端接收缓存信息
        /// <summary>
        /// 该链接的接收缓存
        /// </summary>
        private byte[] clientBuff;
        /// <summary>
        /// 该链接接收缓存的写入位置
        /// </summary>
        private int writeIndex;
        /// <summary>
        /// 该链接接收缓存的读取位置
        /// </summary>
        private int readIndex;
        /// <summary>
        /// 写入状态
        /// </summary>
        private bool writeState;
        /// <summary>
        /// 读取状态
        /// </summary>
        private bool readState;

        //该链接的处理相关类
        /// <summary>
        /// 该链接的处理消息类
        /// </summary>
        private ProcessMsg processMsg;

        //该链接的相关委托
        private DFindSocketByUIDFromPool dFindSocketByUID;
        private DSendMyClientData dSendMsg;

        public int ClientIndex { get => clientIndex; set => clientIndex = value; }
        public bool ClientState { get => clientState; set => clientState = value; }
        public Socket ClientSocket { get => clientSocket; set => clientSocket = value; }
        public long ClientConnStartTime { get => clientConnStartTime; set => clientConnStartTime = value; }
        public int ClientUID { get => clientUID; set => clientUID = value; }


        //该链接相关统计信息
        #region 构造函数
        public ClientConn(int poolIndex,DFindSocketByUIDFromPool dFind)
        {
            this.clientIndex = poolIndex;
            this.clientState = false;
            this.clientUID = -1;
            this.dFindSocketByUID = dFind;
            this.dSendMsg = new DSendMyClientData(BeginSendMsg);
            this.processMsg = new ProcessMsg(dSendMsg,dFindSocketByUID);
        }
        #endregion
        #region 开启客户端链接
        public void OpenClientConn(Socket clientSocket)
        {
            receivePackNumber = 0;

            this.clientSocket = clientSocket;
            this.clientState = true;
            this.clientConnStartTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            this.clientBuff = new byte[MsgConfiguration.MSG_BUFF_SIZE];
            this.writeIndex = 0;
            this.readIndex = 0;
            this.writeState = true;
            this.readState = true;
            clientSocket.BeginReceive(clientBuff, writeIndex, MsgConfiguration.MSG_BUFF_SIZE - writeIndex, SocketFlags.None, EndReceive, null);
            ThreadPool.QueueUserWorkItem(new WaitCallback(CheckMsgInfo));
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientConn] -客户端索引:" + ClientIndex + " -开启新连接");
        }
        #endregion

        #region 接收链接消息
        /// <summary>
        /// 链接异步接收数据完毕
        /// </summary>
        /// <param name="ar">用户定义的对象，其中包含有关接收操作的信息</param>
        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                int receiveCount = clientSocket.EndReceive(ar);

                if (receiveCount == 0)
                {
                    //如果Receive()方法返回0，这个可以作为客户端关闭了的标志

                    return;
                }
                writeIndex += receiveCount;
                //若写入后writeindex=数组大小,writeIndex归零;
                if (writeIndex == MsgConfiguration.MSG_BUFF_SIZE)
                {
                    writeIndex = 0;
                }
                BeginReceiveStart();
            }
            catch(Exception e)
            {
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -接收信息错误:" + e);
                SendCloseMessage("接收信息错误");
            }
        }

        //  一、在写入数据时(接收数据时),socket将从write位置开始异步写入:
        // 1.当writeIndex<readIndex-1,写入到readIndex前一位停止。(写入时writeIndex不可=readIndex，否则会与读取冲突，故实际缓存大小应)
        // 2.当writeIndex>=readIndex,可写入到数组的最后一位。
        // 3.当writeIndex=readIndex-1,进入等待线程池
        // 4.当writeindex=数组大小,writeIndex=0;
        //将此方法与异步接收数据完毕分别写是为了方便委托重启接收
        /// <summary>
        /// 链接开始异步接收数据
        /// </summary>
        private void BeginReceiveStart()
        {
            if (!this.writeState)
            {
                writeState = true;
            }

            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -接收到信息:-readIndex:" +readIndex+"-writeIndex:"+writeIndex);
            //1.当writeIndex<readIndex-1,写入到readIndex-1位停止。
            if (writeIndex < readIndex - 1)
            {
                clientSocket.BeginReceive(clientBuff, writeIndex, (readIndex - writeIndex - 1), SocketFlags.None, EndReceive, null);
            }
            //2.当writeIndex >= readIndex,可写入到数组的最后一位。

            else if (writeIndex >= readIndex)
            {
                if(readIndex==0)
                    clientSocket.BeginReceive(clientBuff, writeIndex, (MsgConfiguration.MSG_BUFF_SIZE - writeIndex-1), SocketFlags.None, EndReceive, null);
                else
                    clientSocket.BeginReceive(clientBuff, writeIndex, (MsgConfiguration.MSG_BUFF_SIZE - writeIndex), SocketFlags.None, EndReceive, null);
            }
            //3.writeIndex=readIndex-1,进入等待
            else if (writeIndex == readIndex - 1)
            {
                this.writeState = false;
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -等待接收重启");
                return;
            }
            RestartCheckMsg();
        }

        /// <summary>
        /// 重启接收 
        /// </summary>
        public void RestartReceive()
        {
            if (!this.writeState)
            {
                BeginReceiveStart();
                writeState = true;
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientConn] -客户端索引:" + ClientIndex + " -重启异步接收");
            }
        }
        #endregion
        #region 处理缓存区消息
        private void CheckMsgInfo(Object obj)
        {
            if (!readState)
            {
                readState = true;
            }

            int checkState;
            bool buffFull = (writeIndex == readIndex - 1);
            MsgPack msgPack=MsgPack.CheckMsgInfo(ref clientBuff, ref readIndex, ref writeIndex, out checkState);
            switch (checkState)
            {
                case 0:
                    {
                        //表示当前无消息
                        Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -等待解析重启:无消息");
                    };break;
                case 1:
                    {
                        //表示检查成功，从中接收数据中解析一个消息包msgPack
                        processMsg.switchMessage(msgPack);
                        receivePackNumber++;
                        Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                            "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -累计处理包裹数量:"+ receivePackNumber);
                        RestartReceive();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(CheckMsgInfo));
                        return;
                    }
                case 2:
                    {
                        //表示消息不完整，需要等待
                        Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -等待解析重启:需要等待");
                    } break;
                case 3:
                    {
                        //表示消息错乱，需要重新连接
                        SendCloseMessage("接收缓存数据紊乱, 需要重连");
                    }
                    break;
            }
            this.readState = false;
        }

        public void RestartCheckMsg()
        {
            if (!readState)
            {
                readState = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(CheckMsgInfo));
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientConn] -客户端索引:" + ClientIndex + " -重启信息解析");
            }
        }
        #endregion

        #region 发送消息
        public void BeginSendMsg(MsgPack msgPack)
        {
            try
            {
                clientSocket.BeginSend(msgPack.GetMsgPackData(), 0, msgPack.GetMsgPackData().Length, SocketFlags.None, asyncResult =>
                {
                    int len = clientSocket.EndSend(asyncResult);
                    Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientNet] -客户端索引:" + ClientIndex + " -发送了一个信息,长度:" + len);
                }, null);
            }
            catch (Exception ex)
            {
                SendCloseMessage("发送消息失败:" + ex);
            }
        }
        #endregion

        #region 关闭链接
        /// <summary>
        /// 给该链接连接的客户端发送关闭链接指令
        /// </summary>
        /// <param name="msg"></param>
        public void SendCloseMessage(string msg)
        {
            CloseTheConn("关闭链接指令:["+msg+"]");
        }

        /// <summary>
        /// 该链接的连接因某些原因被关闭
        /// </summary>
        /// <param name="msg">关闭原因</param>
        public void CloseTheConn(string msg)
        {
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ClientNet] -客户端索引:" + ClientIndex + "-登录用户:{" + clientUID+"}-连接时长:{"+
                (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - clientConnStartTime) + "ms}-关闭了连接,原因:{"+msg+"}");
            this.clientSocket = null;
            this.clientState = false;
            this.clientConnStartTime = -1;
            this.clientBuff = null;
            this.writeIndex = 0;
            this.readIndex = 0;
        }
        #endregion
    }
}
