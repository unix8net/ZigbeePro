using System;
using System.IO;
using System.Windows.Forms;

namespace AccleZigBee
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //定义串口
            System.IO.Ports.SerialPort comm = new System.IO.Ports.SerialPort();
            
            Application.Run(new Start(comm));
        }
        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string strException = string.Format("{0}发生系统异常。\r\n{1}\r\n\r\n\r\n", DateTime.Now, e.ExceptionObject.ToString());
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemException.log"), strException);
        }
    }
}
