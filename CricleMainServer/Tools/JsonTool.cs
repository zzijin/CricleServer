using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CricleMainServer.Tools
{
    class JsonTool
    {
        JObject json;
        public JsonTool()
        {
            json = new JObject();
        }
        public JsonTool(byte[] data)
        {
            string s = Encoding.UTF8.GetString(data);
            json = (JObject)JsonConvert.DeserializeObject(s);
        }
        public JObject getJson()
        {
            return json;
        }
        public int getInt(string name)
        {
            return Convert.ToInt32(json[name]);
        }
        public string getString(string name)
        {
            return Convert.ToString(json[name]);
        }
        public double getDouble(string name)
        {
            return Convert.ToDouble(json[name]);
        }
        /// <summary>
        /// 将json中的时间转换为时间类
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DateTime getDateTime(string name)
        {
            return DateTime.Parse(Convert.ToString(json[name]));
        }
    }
}
