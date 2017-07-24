using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;

namespace DreamExcel.Core
{
    public class ExcelMethod
    {
        /// <summary>
        /// 延迟执行的标记
        /// </summary>
        public static bool IsOver;
        //()
        [ExcelFunction(Description = "如果修改了当前列的变量类型请重新生成脚本后再使用此函数以获取最新的Json列表")]
        public static bool UnityJson()
        {
            var a = CodeDomProvider.CreateProvider("c#", new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
            CompilerParameters cp = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };
            cp.CompilerOptions = "/optimize /target:library";
            cp.ReferencedAssemblies.Add("System.dll");
            try
            {
                var app = WorkBookCore.App;
                var book = app.ActiveWorkbook;
                Worksheet sheet = book.ActiveSheet;
                var cell = (Range)app.Selection;
                var fileName = Path.GetFileNameWithoutExtension(book.Name).Replace(Config.Instance.FileSuffix,"");
                var result = a.CompileAssemblyFromFile(cp, Config.Instance.SaveScriptPath + fileName + ".cs");
                if (result.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in result.Errors)
                    {
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                    }
                    throw new ExcelException(sb.ToString());
                }
                Assembly assembly = result.CompiledAssembly;
                //得到当前列的类型(如果已经生成了脚本的话类名应该是和变量名挂钩的
                var varName = ((Range)sheet.Cells[WorkBookCore.NameRow, cell.Column]).Text;
                Type t = assembly.GetType(Config.Instance.ScriptNameSpace + "." + fileName + "Property");
                t = ((PropertyInfo)t.GetProperty(varName)).PropertyType;
                //因为Function中无法操作单元格所以等待Function结束后再清除单元格进行赋值
                Task.Run(() =>
                {
                    while (IsOver)
                    {
                        IsOver = false;
                    }
                    try
                    {
                        if (!t.IsArray)
                            cell.Value = JsonConvert.SerializeObject(Activator.CreateInstance(t), new JsonSerializerSettings {ContractResolver = new SpecialContractResolver()});
                        else
                        {
                            var arrayInstance = Array.CreateInstance(t.GetElementType(), 1);
                            arrayInstance.SetValue(Activator.CreateInstance(t.GetElementType()),0);
                            cell.Value = JsonConvert.SerializeObject(arrayInstance, new JsonSerializerSettings { ContractResolver = new SpecialContractResolver() });
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                });
                return IsOver = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return IsOver = true;
        }
    }
}