using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;
using ScriptGenerate;
using SQLite4Unity3d;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace DreamExcel.Core
{
    public class WorkBookCore: IExcelAddIn
    {
        /// <summary>
        /// 绑定
        /// </summary>
        public static Application App = (Application)ExcelDnaUtil.Application;
        /// <summary>
        /// 第X行开始才是正式数据
        /// </summary>
        private const int StartLine = 4;
        /// <summary>
        ///     关键Key值,这个值在Excel表里面必须存在
        /// </summary>
        private const string Key = "Id";

        /// <summary>
        /// 类型的所在行
        /// </summary>
        public const int TypeRow = 3;
        /// <summary>
        /// 名称的所在行
        /// </summary>
        public const int NameRow = 2;

        public static HashSet<string> SupportType = new HashSet<string>
        {
            "int","string","bool","long","float","int[]","string[]","long[]","bool[]","float[]"
        };

        internal static Dictionary<string, string> FullTypeSqliteMapping = new Dictionary<string, string>
        {
            {"int","INTEGER" },
            {"string","TEXT" },
            {"float","REAL" },
            {"bool","INTEGER" },
            {"long","INTEGER" },
            {"int[]","TEXT"},
            {"string[]","TEXT"},
            {"float[]","TEXT"},
            {"bool[]","TEXT"},
            {"long[]","TEXT"},
        };

        private void Workbook_BeforeSave(Workbook wb,bool b, ref bool r)
        {
            if (wb.IsVaildWorkBook()) return;
            var activeSheet = (Worksheet) wb.ActiveSheet;
            var fileName = Path.GetFileNameWithoutExtension(wb.Name).Replace(Config.Instance.FileSuffix, "");
            var dbDirPath = Config.Instance.SaveDbPath;
            var dbFilePath = dbDirPath + fileName + ".db";
            if (!Directory.Exists(dbDirPath))
            {
                Directory.CreateDirectory(dbDirPath);
            }
            Range usedRange = activeSheet.UsedRange;
            var rowCount = usedRange.Rows.Count;
            var columnCount = usedRange.Columns.Count;
            List<TableStruct> table = new List<TableStruct>();
            bool haveKey = false;
            string keyType = "";
            object[,] cells = usedRange.Value2;
            List<int> passColumns = new List<int>(); //跳过列
            for (var index = 1; index < columnCount + 1; index++)
            {
                //从1开始,第0行是策划用来写备注的地方第1行为程序使用的变量名,第2行为变量类型
                string t1 = Convert.ToString(cells[NameRow, index]);
                if (string.IsNullOrWhiteSpace(t1))
                {
                    var cell = ((Range)usedRange.Cells[NameRow, index]).Address;
                    throw new ExcelException("单元格:" + cell + "名称不能为空");
                }
                if (t1.StartsWith("*"))
                {
                    passColumns.Add(index);
                    continue;
                }
                string type = Convert.ToString(cells[TypeRow, index]);
                if (t1 == Key)
                {
                    haveKey = true;
                    keyType = type;
                    if (keyType!="int" && keyType != "string")
                    {
                        throw new ExcelException("表ID的类型不支持,可使用的类型必须为 int,string");
                    }
                }
                table.Add(new TableStruct(t1, type));
            }
            if (!haveKey)
            {
                throw new ExcelException("表格中不存在关键Key,你需要新增一列变量名为" + Key + "的变量作为键值");
            }
            try
            {
                //生成C#脚本
                var customClass = new List<GenerateConfigTemplate>();
                var coreClass = new GenerateConfigTemplate {Class = new GenerateClassTemplate {Name = fileName, Type = keyType}};
                for (int i = 0; i < table.Count; i++)
                {
                    var t = table[i];
                    if (!SupportType.Contains(t.Type))
                    {
                        var newCustomType = TableAnalyzer.GenerateCustomClass(t.Type, t.Name);
                        coreClass.Add(new GeneratePropertiesTemplate {Name = t.Name, Type = newCustomType.Class.Name + (t.Type.StartsWith("{") ? "[]" : "")});
                        customClass.Add(newCustomType);
                    }
                    else
                    {
                        coreClass.Add(new GeneratePropertiesTemplate {Name = t.Name, Type = t.Type});
                    }
                }
                CodeGenerate.Start(customClass, coreClass, fileName);
            }
            catch (Exception e)
            {
                throw new ExcelException("生成脚本失败\n" + e);
            }
            try
            {
                if (File.Exists(dbFilePath))
                {
                    File.Delete(dbFilePath);
                }
            }
            catch(Exception e)
            {
                throw new ExcelException("无法写入数据库至" + dbFilePath + "请检查是否有任何应用正在使用该文件");
            }
            using (var conn = new SQLiteConnection(dbFilePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    var tableName = fileName;
                    SQLiteCommand sql = new SQLiteCommand(conn);
                    sql.CommandText = "PRAGMA synchronous = OFF";
                    sql.ExecuteNonQuery();
                    //创建关键Key写入表头
                    sb.Append("create table if not exists " + tableName + " (" + Key + " " + FullTypeSqliteMapping[keyType] + " PRIMARY KEY not null, ");
                    for (int n = 0; n < table.Count; n++)
                    {
                        if (table[n].Name == Key)
                            continue;
                        var t = FullTypeSqliteMapping.ContainsKey(table[n].Type);
                        string sqliteType = t ? FullTypeSqliteMapping[table[n].Type] : "TEXT";
                        sb.Append(table[n].Name + " " + sqliteType + ",");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(")");
                    sql.CommandText = sb.ToString();
                    sql.ExecuteNonQuery();
                    //准备写入表内容
                    sb.Clear();
                    conn.BeginTransaction();
                    object[] writeInfo = new object[columnCount];
                    for (int i = StartLine; i <= rowCount; i++)
                    {
                        int offset = 1;
                        for (var n = 1; n <= columnCount; n++)
                        {
                            try
                            {
                                if (passColumns.Contains(n))
                                {
                                    offset++;
                                    continue;
                                }
                                var property = table[n - offset];
                                string cell = Convert.ToString(cells[i, n]);
                                if (table.Count > n - offset)
                                {
                                    string sqliteType;
                                    if (FullTypeSqliteMapping.TryGetValue(property.Type, out sqliteType)) //常规类型可以使用这种方法直接转换
                                    {
                                        var attr = TableAnalyzer.SplitData(cell);
                                        if (property.Type == "System.Boolean")
                                            writeInfo[n - offset] = attr[0].ToUpper() == "TRUE" ? 0 : 1;
                                        else if (sqliteType != "TEXT")
                                            writeInfo[n - offset] = attr[0];
                                        else
                                            writeInfo[n - offset] = cell;
                                    }
                                    else
                                    {
                                        //自定义类型序列化
                                        writeInfo[n - 1] = cell;
                                    }
                                }
                            }
                            catch
                            {
                                throw new Exception("单元格:" + ((Range) usedRange.Cells[i, n]).Address + "存在异常");
                            }
                        }
                        sb.Append("replace into " + fileName + " ");
                        sb.Append("(");
                        foreach (var node in table)
                        {
                            sb.Append(node.Name + ",");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(") values (");
                        for (var index = 0; index < table.Count; index++)
                        {
                            sb.Append("?,");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(")");
                        conn.CreateCommand(sb.ToString(), writeInfo).ExecuteNonQuery();
                        sb.Clear();
                    }
                    conn.Commit();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        public void AutoOpen()
        {

            App.WorkbookOpen += wb =>
            {
                for (int i = 0; i < Config.Instance.SupportCustomType.Length; i++)
                {
                    AddSupportType(Config.Instance.SupportCustomType[i]);
                }
            };
            App.WorkbookBeforeSave += Workbook_BeforeSave;
        }

        public void AutoClose()
        {
        }

        public void AddSupportType(string str)
        {
            var support = str.Split(':');
            SupportType.Add(support[0]);
            FullTypeSqliteMapping.Add(support[0],support[1]);
        }
    }
}



