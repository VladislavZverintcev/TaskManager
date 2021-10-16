using Newtonsoft.Json;

namespace TaskManager.Model
{
    class JsonWorker
    {
        public static string GetSerializedObj(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static object GetDeserializedObj(string jString)
        {
            if (jString == null)
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject(jString);
            }
        }
    }
}
