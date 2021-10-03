using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TaskManager.Model
{
    class JsonWorker
    {
        public static string GetSerializedObj(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        public static object GetDeserializedObj(string jString)
        {
            if (jString == null)
            {
                return null;
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(jString);
            }
        }
    }
}
