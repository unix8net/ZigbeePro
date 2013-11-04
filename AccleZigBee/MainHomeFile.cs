using System;
using System.Diagnostics;//
using System.IO;
using System.Windows.Forms;
using System.Threading;
namespace AccleZigBee
{
    public partial class MainHome
    {


        private void checkFile()
        {

            //检查数据文件夹
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory;
            tpath = tpath + @"\Data\";
            if (!Directory.Exists(tpath))
                Directory.CreateDirectory(tpath);
            //检查日志文件
            tpath = System.AppDomain.CurrentDomain.BaseDirectory;
            tpath = tpath + @"\log.txt";
            if (!File.Exists(tpath))
                File.CreateText(tpath).Close();

            //检查角度文件夹是否存在，不存在则创建
            tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\";
            
            if (!Directory.Exists(tpath))
                Directory.CreateDirectory(tpath);
            else
            {
                Directory.Delete(tpath, true);
                Directory.CreateDirectory(tpath);
            }

            tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\config.txt";
            if (!File.Exists(tpath))
            {
                MessageBox.Show("没有在安装目录下找到config.txt标定文件,请配置");
            }



            //读取当前excel已经写了的行数
            tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\line.txt";
            //存在该文件则读取，不存在则创建
            if (File.Exists(tpath))
            {
                using (StreamReader sr = new StreamReader(tpath))
                {
                    tpath = sr.ReadLine();
                    if (tpath != null)
                    {
                        ecxelLine = Convert.ToInt16(tpath);
                        tpath = sr.ReadLine();
                        excelName = tpath;
                    }
                    else
                    {
                        ecxelLine = 2;
                        excelName = "data1";
                    }
                    sr.Close();
                }//using
            }
            else
            {
                File.CreateText(tpath).Close();
                using (StreamWriter sw = File.AppendText(tpath))
                {
                    sw.WriteLine("2");
                    sw.WriteLine("data1");
                    excelName = "data1";
                    ecxelLine = 2;
                    sw.Close();
                }
            }
        }
        #region 读取文件来配置端口与名字
        public string readMacByName(string name)
        {
            //IEEE与NAME映射文件 [MAC+NAME]
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\IEEE.txt";
            if (!File.Exists(tpath))
                return null;
            using (StreamReader sr = new StreamReader(tpath))
            {
                string line1, line2;
                //line1读MAC地址
                while ((line1 = sr.ReadLine()) != null)
                {
                    //line2读取名字
                    line2 = sr.ReadLine();
                    //根据IEEE地址来找名字
                    if (line2 == name)
                    {
                        sr.Close();
                        return line1;
                    }
                }//while

            }//using
            return null;

        }
        private string readNameByMac(string mac)
        {
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"Config\IEEE.txt";
            if (!File.Exists(tpath))
                return null;
            using (StreamReader sr = new StreamReader(tpath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    //根据IEEE地址来找名字
                    if (line == mac)
                    {
                        line = sr.ReadLine();
                        sr.Close();
                        return line;
                    }
                }//while

            }//using
            return null;
        }
        #endregion

        #region 创建数据文件
        public string creatMyDataText(string name)
        {
            string angle = "NULL";
            //原始数据文件
            string myDataPath = System.AppDomain.CurrentDomain.BaseDirectory;
            myDataPath = myDataPath + @"\Data\" + name + @"\OR" + DateTime.Now.Year.ToString() + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" + DateTime.Now.Day.ToString().PadLeft(2, '0')+"日" + @"\";
            if (!Directory.Exists(myDataPath))
                Directory.CreateDirectory(myDataPath);
            myDataPath += /*DateTime.Now.Month.ToString()+"月"+DateTime.Now.Day.ToString() + "日" +*/ DateTime.Now.Hour.ToString().PadLeft(2, '0')
                + "时" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + "分" + DateTime.Now.Second.ToString().PadLeft(2, '0')+"秒";

            //当前节点的数据文件
            string anglePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + name;
            if (File.Exists(anglePath))
            {   //如果是统一角度
                if (isSameAngle)
                {
                    //先清空文件
                    FileStream stream2 = File.Open(anglePath, FileMode.OpenOrCreate, FileAccess.Write);
                    stream2.Seek(0, SeekOrigin.Begin);
                    stream2.SetLength(0); //清空txt文件
                    stream2.Close();
                    //再将统一的角度写入角度文件
                    using (StreamWriter sw = File.AppendText(anglePath))
                    {
                        angle = sameAngle.ToString();
                        sw.WriteLine(angle);
                        sw.Close();
                    }
                    
                }
                else
                { //如果不是统一角度，则读取角度文件来获得角度
                    using (StreamReader sr = new StreamReader(anglePath))
                    {
                        angle = sr.ReadLine();
                        if (angle == null)
                            angle = "NULL";
                        sr.Close();
                    }//using
                }
            }
            else
            {
                File.CreateText(anglePath).Close();
                if (isSameAngle)
                {
                    using (StreamWriter sw = File.AppendText(anglePath))
                    {
                        angle = sameAngle.ToString();
                        sw.WriteLine(angle);
                        sw.Close();
                    }
                }
                else
                {
                    angle = "NULL";
                }
            }


            //将角度写入原始数据文件
            if (!File.Exists(myDataPath))
                File.CreateText(myDataPath).Close();
            using (StreamWriter sw = File.AppendText(myDataPath))
            {
                sw.WriteLine(angle);
                sw.Close();
            }
            return myDataPath;

        }
        public string creatDataTxt(string name)
        {
            
            string angle = "NULL";

            string anglePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + name;
            if (File.Exists(anglePath))
            {
                using (StreamReader sr = new StreamReader(anglePath))
                {
                    angle = sr.ReadLine();
                    if (angle == null)
                        angle = "NULL";
                    sr.Close();
                }//using
            }
            else
            {
                angle = "NULL";
            }
            /*
            if (!File.Exists(tpath))
                File.CreateText(tpath).Close();
            using (StreamWriter sw = File.AppendText(tpath))
            {
                sw.WriteLine(angle);
                sw.Close();
            }
            */
            string nowTime = DateTime.Now.Year.ToString() + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" +
                    DateTime.Now.Day.ToString().PadLeft(2, '0') + "日" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + "时" +
                    DateTime.Now.Minute.ToString().PadLeft(2, '0') + "分" + DateTime.Now.Second.ToString().PadLeft(2, '0')+"秒";
            try
            {
                //行数太多，准备换文件存放
                if (ecxelLine >= 2000)
                {
                    excel.RealeseResource();
                    excelName = excelName.Replace("data", "");
                    excelName = "data" + (UInt16.Parse(excelName) + 1).ToString();
                    //关闭节点
                    sendClose();
                    Thread.Sleep(4000);
                    //再重启
                    initExcel(excelName+".xls");
                    ecxelLine = 2;
                    return null;
#if false
                    if (ringFlag)
                    {
                        button2.PerformClick();
                    }
                    else if(cycleFlag)
                    {
                        button3.PerformClick();
                    }
#endif
                }
               // Console.WriteLine("在{0}新建一条记录,行数为{1}", excelName, ecxelLine);

                excel.AddData<string>(ecxelLine, 1, name);
                excel.AddData<string>(ecxelLine, 2, nowTime/*DateTime.Now.ToLocalTime().ToString()*/);
                excel.AddData<string>(ecxelLine, 3, angle);
                excel.AddData<string>(ecxelLine, 4, "NULL");
                excel.SaveData();
            }
            catch(Exception)
            {
               // MessageBox.Show("请关闭data.xls,在软件中使用转存后打开data_copy.xls,软件稍后需要使用data.xls");
                Notify notify = new Notify("请关闭data.xls,在软件中使用转存后打开data_copy.xls,软件稍后需要使用data.xls");
                notify.ShowDialog();
                
                try
                {
                    excel.RealeseResource();
                    initExcel(excelName+".xls");
                    
                    excel.AddData<string>(ecxelLine, 1, name);
                    //excel.AddData<string>(ecxelLine, 2, DateTime.Now.ToLocalTime().ToString());
                    excel.AddData<string>(ecxelLine, 2, nowTime/*DateTime.Now.ToLocalTime().ToString()*/);
                    excel.AddData<string>(ecxelLine, 3, angle);
                    excel.AddData<string>(ecxelLine, 4, "NULL");
                    excel.SaveData();
                }
                catch (Exception)
                { }
            }
            return null;
        }
        #endregion


        private void Log(string name)
        {

            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            path = path + @"\log.txt";

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("物理地址为 " + name + " " + DateTime.Now.ToString() + "上线");
                sw.Close();
            }
        }

        public int getIndexByName(string name)
        {
            int value;
            if (nodeNameIndexHash.TryGetValue(name, out value))
            {
                return value;

            }
            else
                return - 1;
        }
        #region 杀死进程
        private void KillProcess(string processName)
        {
            //获得进程对象，以用来操作
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            //得到所有打开的进程 
            try
            {
                //获得需要杀死的进程名
                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    //立即杀死进程
                    thisproc.Kill();
                }
            }
            catch (Exception Exc)
            {
                throw new Exception("", Exc);
            }
        }
        #endregion 
    }
}
