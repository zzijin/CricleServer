using System;
using System.Text;

namespace CricleMainServer.Network
{
    class ProcessMsg
    {
        private DSendMyClientData dSendMsg;
        private DFindSocketByUIDFromPool dFindSocketByUID;

        public ProcessMsg(DSendMyClientData sendMsg,DFindSocketByUIDFromPool dFind)
        {
            this.dSendMsg = sendMsg;this.dFindSocketByUID = dFind;
        }

        public bool switchMessage(MsgPack msgPack)
        {
            MsgBasic msgBasic = msgPack.MsgData;
            ConsolePro(msgBasic);
            switch (msgBasic.MsgType)
            {
                case 0:dSendMsg(new MsgPack(new MsgBasic(0, Encoding.UTF8.GetBytes(" 测试连接"), 1)));break;

            }
            return false;
        }

        private void ConsolePro(MsgBasic msgBasic)
        {
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() + "] [Class:ProcessMsg] -Data:"
                + Encoding.UTF8.GetString(msgBasic.MsgData) + " - Type:" + msgBasic.MsgType + " -Flag:" + msgBasic.MsgFlag +
                " -Time:" + msgBasic.MsgTime);
        }
    }
}
