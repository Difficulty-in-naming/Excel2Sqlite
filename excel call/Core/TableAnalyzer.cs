using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DreamExcel.Core
{
    public static class TableAnalyzer
    {
        /// <summary>
        ///     拆分格子数据
        /// </summary>
        public static string[] SplitData(string data)
        {
            data = Regex.Replace(data, "\"\"", "\"");
            var count = 0;
            var stringData = data;
            if (data.StartsWith("\""))
                stringData = stringData.Remove(stringData.Length - 1, 1).Remove(0, 1);
            var sb = new StringBuilder();
            var dataList = new List<string>();
            for (var i = 0; i < stringData.Length; i++)
            {
                if (stringData[i] == '\"')
                {
                    count++;
                }
                else if (count % 2 == 0 && stringData[i] == ',')
                {
                    count = 0;
                    dataList.Add(sb.ToString());
                    sb = new StringBuilder();
                    continue;
                }
                sb.Append(stringData[i]);
                if (i == stringData.Length - 1)
                    dataList.Add(sb.ToString());
            }
            return dataList.ToArray();
        }

        public static bool CheckType(string str)
        {
            return WorkBookCore.FullTypeSqliteMapping.ContainsKey(str);

        }
    }
}

