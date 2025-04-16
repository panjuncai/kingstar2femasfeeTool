using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace kingstar2femasfee
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 设置EPPlus 8.0许可证
            // 根据PolyForm Noncommercial许可证要求设置为非商业用途
            // 参考: https://polyformproject.org/licenses/noncommercial/1.0.0/
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage.License.SetNonCommercialPersonal("Jerry"); 
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 初始化数据库
            DatabaseHelper.InitializeDatabase();
            Application.Run(new MainForm());
        }
    }
}
