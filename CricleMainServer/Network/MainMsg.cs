using CricleMainServer.Network.NetInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network
{
    ///*************************************************** 消息包格式
    ///     是否使用              名称                  变量名              类型         长度(字节)            说明
    ///        Y              消息包开始标识符        msgStartTag           byte             1                 包头标识，固定值
    ///        Y              消息包大小              msgSize               int32            4                 该消息包长度,MainMsg总长度
    ////////////MainMsg包含:        
    ///        Y              操作号/消息类别         msgType               int32            4                 该消息包操作号，用于对相应消息包进行处理
    ///        Y              消息主体/数据           msgData               byte[]         不定长              该消息包的主体数据
    ///        N              时间戳                  msgTime               long             8                 该消息包发出时的时间戳
    ///        Y              消息数据加密标志        msgFlag               int32            4                 该消息包主体数据的加密方式
    ////////////以上
    ///        Y              消息包结束标识符        msgEndTag             byte             1                 包尾标识符
    ///*************************************************** 消息包格式

    /// <summary>
    /// socket收发TCP主体消息类
    /// </summary>
    class MainMsg :IMainMsg
    {
        private int msgType;
        private byte[] msgData;
        private long msgTime;
        private int msgFlag;


    }
}
