using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;

namespace DreamExcel.Core
{
    public class ExcelMethod
    {
        /// <summary>
        /// 延迟执行的标记
        /// </summary>
        public static bool IsOver;
        //()
        [ExcelFunction(Description = "快速生成一个Json模板")]
        public static bool UnityJson()
        {
            try
            {
                var app = WorkBookCore.App;
                var cell = (Range)app.Selection;
                //因为Function中无法操作单元格所以等待Function结束后再清除单元格进行赋值
                Task.Run(() =>
                {
                    while (IsOver)
                    {
                        IsOver = false;
                    }
                    try
                    {
                        cell.Value = JsonGenerate.Serialize(((Range)app.ActiveSheet.Cells[WorkBookCore.TypeRow, cell.Column]).Text);
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