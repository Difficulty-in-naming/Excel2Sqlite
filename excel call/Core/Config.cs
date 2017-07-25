using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DreamExcel.Core
{
    public class Config
    {
        public static Config Instance
        {
            get
            {
                var mInstance = new Config();
                //如果工作目录下存在配置文件则读取工作目录下的配置
                var configPath = WorkBookCore.App.ActiveWorkbook.Path + "/Config.txt";
                string content;
                if (File.Exists(configPath))
                {
                    content = File.ReadAllText(configPath);
                }
                else
                {
                    content = File.ReadAllText(CurrentPath + "/Config.txt");
                }
                content = Regex.Replace(content, @"\/\*((?:[^*]|(?:\*(?=[^\/])))*)\*\/", "");
                content = content.Replace("\n", "").Replace("\r","");
                var split = content.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].StartsWith(nameof(SaveScriptPath)))
                    {
                        mInstance.SaveScriptPath = GetValue(split[i]);
                    }
                    else if (split[i].StartsWith(nameof(SaveDbPath)))
                    {
                        mInstance.SaveDbPath = GetValue(split[i]);
                    }
                    else if (split[i].StartsWith(nameof(ScriptNameSpace)))
                    {
                        mInstance.ScriptNameSpace = GetValue(split[i]);
                    }
                    else if (split[i].StartsWith(nameof(FileSuffix)))
                    {
                        mInstance.FileSuffix = GetValue(split[i]);
                    }
                }
                return mInstance;
            }
        }

        private static string GetValue(string split)
        {
            return split.Substring(split.IndexOf("=") + 1).Trim();
        }

        private static string CurrentPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        public string ScriptTemplatePath
        {
            get
            {
                var configPath = WorkBookCore.App.ActiveWorkbook.Path + "/GenerateTemplate.txt";
                if (File.Exists(configPath))
                {
                    return configPath;
                }
                else
                {
                    return CurrentPath + "/GenerateTemplate.txt";
                }
            }
        }

        private string mSaveScriptPath;
        public string SaveScriptPath
        {
            get
            {
                if (mSaveScriptPath.Contains(":")) //盘符标志
                {
                    return mSaveScriptPath;
                }
                return WorkBookCore.App.ActiveWorkbook.Path + "\\" + mSaveScriptPath;
            }
            private set { mSaveScriptPath = value; }
        }

        public string ScriptNameSpace { get; private set; }
        public string FileSuffix { get; private set; }
        private string mSaveDbPath;

        public string SaveDbPath
        {
            get
            {
                if (mSaveDbPath.Contains(":")) //盘符标志
                {
                    return mSaveDbPath;
                }
                return WorkBookCore.App.ActiveWorkbook.Path + "\\" + mSaveDbPath;
            }
            set { mSaveDbPath = value; }
        }
    }
}
