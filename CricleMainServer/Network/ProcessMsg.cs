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
            switch (msgBasic.MsgType)
            {
                case 0:;break;

            }
            return false;
        }


    }
}
