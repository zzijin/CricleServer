using CricleMainServer.Network;
using CricleMainServer.Network.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CricleMainServer.TestConsole
{
    class TestReceiveAndProcesses
    {
        private static byte[] testReceivedDataArray = new byte[MsgConfiguration.MSG_BUFF_SIZE];
        private static int readIndex = 0;
        private static int writeIndex = 0;

        public static void tMain()
        {
            TestMsgPack();
        }

        /// <summary>
        /// 模拟接收解析数据
        /// </summary>
        static void TestMsgPack()
        {
            //WriteMsgPack(4);
            ThreadPool.SetMinThreads(2, 2);
            ThreadPool.SetMaxThreads(4, 4);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadWriteMsgPack));
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadReadMsgPack));
        }

        static void ThreadWriteMsgPack(Object obj)
        {
            while (true)
            {
                Thread.Sleep(5);
                WriteMsgPack(new Random().Next(1, 3));
            }
        }

        static void ThreadReadMsgPack(Object obj)
        {
            while (true)
            {
                Thread.Sleep(1);
                ReadMsgPack(1);
            }
        }

        //模拟接收数据
        static void WriteMsgPack(int size)
        {
            for (int i = 0; i < size; i++)
            {
                JObject json = new JObject();
                json.Add("Data", "测试拆包:这是包裹" + i);
                MsgBasic msgBasic1 = new MsgBasic(0, Encoding.UTF8.GetBytes(json.ToString()), 1);
                MsgPack msgPack1 = new MsgPack(msgBasic1);
                Console.WriteLine("***************Pack***************");
                Console.WriteLine("StartTag:" + msgPack1.MsgStartTag);
                Console.WriteLine("MsgSize:" + msgPack1.MsgSize);
                Console.WriteLine("EndTag" + msgPack1.MsgEndTag);
                Console.WriteLine("Type:" + msgBasic1.MsgType);
                JObject jObject1 = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msgBasic1.MsgData));
                Console.WriteLine("Data:" + Convert.ToString(jObject1["Data"]));
                Console.WriteLine("Time:" + msgBasic1.MsgTime);
                Console.WriteLine("Flag:" + msgBasic1.MsgFlag);
                TestReceivedData(msgPack1.GetMsgPackData());
            }
        }
        //模拟解析数据
        static void ReadMsgPack(int size)
        {
            for (int i = 0; i < size; i++)
            {
                int state;
                MsgPack msgPack2 = MsgPack.CheckMsgInfo(ref testReceivedDataArray, ref readIndex, ref writeIndex, out state);
                if (state == 1)
                {
                    Console.WriteLine("***************UnPack***************");
                    Console.WriteLine("StartTag:" + msgPack2.MsgStartTag);
                    Console.WriteLine("MsgSize:" + msgPack2.MsgSize);
                    Console.WriteLine("EndTag" + msgPack2.MsgEndTag);
                    Console.WriteLine("Type:" + msgPack2.MsgData.MsgType);
                    JObject jObject2 = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msgPack2.MsgData.MsgData));
                    Console.WriteLine("Data:" + Convert.ToString(jObject2["Data"]));
                    Console.WriteLine("Time:" + msgPack2.MsgData.MsgTime);
                    Console.WriteLine("Flag:" + msgPack2.MsgData.MsgFlag);
                }
                else
                {
                    Console.WriteLine("***************UnPack***************");
                    Console.WriteLine("解析失败，因为state=" + state);
                }
            }
            Console.WriteLine("***************执行完毕***************");
        }

        /// <summary>
        /// 模拟接收数据到数组 
        /// </summary>
        /// <param name="testReceivedData"></param>
        /// <param name="testReadIndex"></param>
        /// <param name="testWriteData"></param>
        static void TestReceivedData(byte[] testData)
        {
            int size = testData.Length;
            if (writeIndex < readIndex - 1)
            {
                if (size <= readIndex - writeIndex)
                {
                    Buffer.BlockCopy(testData, 0, testReceivedDataArray, writeIndex, size);
                    writeIndex += size;
                    Console.WriteLine("写入成功-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "写入:" + size);
                    return;
                }
                else
                {
                    Console.WriteLine("写入溢出-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "-需写入:" + size + "-可写入:" + (readIndex - writeIndex));
                    return;
                }
            }
            //2.当writeIndex >= readIndex,可写入到数组的最后一位。
            else if (writeIndex >= readIndex)
            {
                if (size <= (MsgConfiguration.MSG_BUFF_SIZE - writeIndex))
                {
                    Buffer.BlockCopy(testData, 0, testReceivedDataArray, writeIndex, size);
                    writeIndex += size;
                    Console.WriteLine("写入成功-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "写入:" + size);
                    return;
                }
                else if (size > (MsgConfiguration.MSG_BUFF_SIZE - writeIndex))
                {
                    if (MsgConfiguration.MSG_BUFF_SIZE - writeIndex + readIndex >= size)
                    {
                        int writeSize1 = MsgConfiguration.MSG_BUFF_SIZE - writeIndex;
                        int writeSize2 = size - (MsgConfiguration.MSG_BUFF_SIZE - writeIndex);
                        Buffer.BlockCopy(testData, 0, testReceivedDataArray, writeIndex, writeSize1);
                        Buffer.BlockCopy(testData, writeSize1, testReceivedDataArray, 0, writeSize2);
                        writeIndex = writeSize2;
                        Console.WriteLine("写入成功-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "写入:" + size);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("写入溢出-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "-需写入:" + size + "-可写入:" + (MsgConfiguration.MSG_BUFF_SIZE - writeIndex + readIndex));
                        return;
                    }
                }
            }
            //3.writeIndex=readIndex-1,进入等待
            //重启接收:由委托重启
            else if (writeIndex == readIndex - 1)
            {
                Console.WriteLine("写入溢出-ReadIndex:" + readIndex + "-WriteIndex:" + writeIndex + "-需写入:" + size + "-可写入:" + (MsgConfiguration.MSG_BUFF_SIZE - writeIndex + readIndex));
                return;
            }
        }

        /// <summary>
        /// 验证基础包解包拆包数据是否一致
        /// </summary>
        static void TestMsgBasic()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            JObject json = new JObject();
            json.Add("Data", "测试拆包");
            MsgBasic msgBasic1 = new MsgBasic(0, Encoding.UTF8.GetBytes(json.ToString()), Convert.ToInt64(ts.TotalMilliseconds), 1);

            MsgPack msgPack1 = new MsgPack(0x99, msgBasic1, 0x99);
            Console.WriteLine("***************Old***************");
            Console.WriteLine("StartTag:" + msgPack1.MsgStartTag);
            Console.WriteLine("MsgSize:" + msgPack1.MsgSize);
            Console.WriteLine("EndTag" + msgPack1.MsgEndTag);
            Console.WriteLine("Type:" + msgBasic1.MsgType);
            JObject jObject1 = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msgBasic1.MsgData));
            Console.WriteLine("Data:" + Convert.ToString(jObject1["Data"]));
            Console.WriteLine("Time:" + msgBasic1.MsgTime);
            Console.WriteLine("Flag:" + msgBasic1.MsgFlag);

            MsgPack msgPack2 = new MsgPack(msgPack1.GetMsgPackData());
            MsgBasic msgBasic2 = msgPack2.MsgData;

            Console.WriteLine("***************New***************");
            Console.WriteLine("StartTag:" + msgPack2.MsgStartTag);
            Console.WriteLine("MsgSize:" + msgPack2.MsgSize);
            Console.WriteLine("EndTag" + msgPack2.MsgEndTag);
            Console.WriteLine("Type:" + msgBasic2.MsgType);
            JObject jObject2 = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msgBasic2.MsgData));
            Console.WriteLine("Data:" + Convert.ToString(jObject2["Data"]));
            Console.WriteLine("Time:" + msgBasic2.MsgTime);
            Console.WriteLine("Flag:" + msgBasic2.MsgFlag);
            Console.ReadLine();
        }
    }
}
