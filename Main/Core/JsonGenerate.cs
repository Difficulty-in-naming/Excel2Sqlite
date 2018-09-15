using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DreamExcel.Core
{
    public static class JsonGenerate
    {
        public static string Serialize(string type)
        {
            var customClass = TableAnalyzer.GenerateCustomClass(type).Item1;
            bool isArray = type.StartsWith("{") && type.EndsWith("}");
            StringBuilder sb = new StringBuilder();
            sb.Append(isArray ? "[" : "");
            sb.Append("{");
            var properties = customClass.Properties;
            for (int i = 0; i < properties.Count; i++)
            {
                sb.Append("\"");
                sb.Append(customClass.Properties[i].Name);
                sb.Append("\"");
                sb.Append(":");
                sb.Append(GetDefault(properties[i].Type));
                if(i != properties.Count - 1)
                    sb.Append(",");
            }
            sb.Append("}");
            sb.Append(isArray ? "]" : "");
            return sb.ToString();
        }

        private static string GetDefault(string str)
        {
            if (str == "int" || str == "float" || str == "long")
                return "0";
            else if (str == "string")
                return "\"\"";
            else if (str == "bool")
                return "true";
            else if (str.Contains("[]"))
                return "[]";
            else
                throw new Exception("不支持的类型");
        }

    }
}
