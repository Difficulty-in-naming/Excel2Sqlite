using System;
using System.IO;

namespace DreamExcel.Core
{
    public class Config
    {
        //不知道为什么mInstance默认就有值了.
        private static Config mInstance;
        public static Config Instance
        {
            get
            {
/*                if (mInstance != null)
                    return mInstance;*/
                mInstance = new Config();
                var content = File.ReadAllText(CurrentPath + "/Config.txt");
                content = content.Replace("\n", "").Replace("\r","");
                var split = content.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].StartsWith(nameof(ScriptTemplatePath)))
                    {
                        mInstance.ScriptTemplatePath = CurrentPath + "\\" + GetValue(split[i]);
                    }
                    else if (split[i].StartsWith(nameof(SaveScriptPath)))
                    {
                        mInstance.SaveScriptPath = GetValue(split[i]);
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
        public string ScriptTemplatePath { get; private set; }
        private string mSaveScriptPath;
        public string SaveScriptPath
        {
            get { return WorkBookCore.App.ActiveWorkbook.Path + "\\" + mSaveScriptPath; }
            private set { mSaveScriptPath = value; }
        }

        public string ScriptNameSpace { get; private set; }
        public string FileSuffix { get; private set; }
    }
}
