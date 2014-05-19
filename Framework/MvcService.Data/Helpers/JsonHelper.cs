using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcService.Data.Helpers
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }
    }

    public class JsonHelper
    {
        private static readonly Type _defaultType = typeof(object);

        public static string ConvertToJson(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static object ParseJson(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static object ParseJson(byte[] json, Type type)
        {
            return ParseJson(Encoding.ASCII.GetString(json), type);
        }

        public static object ParseJson(string json)
        {
            return ParseJson(json, _defaultType);
        }

        public static object ParseJson(byte[] json)
        {
            return ParseJson(Encoding.ASCII.GetString(json));
        }
    }
}
