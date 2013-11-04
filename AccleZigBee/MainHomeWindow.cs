using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class MainHome
    {
        private void initial()
        {
            //检查必须文件是否存在
            checkFile();

            //3个object为同步变量，用于串口模块并发
            lm = new object();
            lo = new object();
            ls = new object();
            //气泡提示
            tips = new ToolTip();
            //滤波标志
            filter = true;
            //标定标志
            modify = true;
            //记录日志标志
            logData = true;
            //周期发送开闭标志，多点周期的标志
            cycleFlag = false;
            //记录是否是第一次多点周期循环，因为第一次总是需要让设备处于打开状态
            isFirstCycleFlag = false;
            //循环发送标志
            ringFlag = false;
            oneFlag = false;
            //manuallyFlag = false;
            //软件启动后，界面上显示的灰色图标的标记，第一次使用后变失效
            noDataWithGrayPic = false;
            //记录打开的加速度节点 （终端）编号，用于波形显示
            openingEndIndnex = 0;
            //记录打开的网关（路由器）节点便后，用于显示他的儿子节点
            openingRouterIndex = 0;
            //count用于该系统的心跳，每一秒触发一次，也作为整个系统的软件时钟，所有时间都是从count中获取
            count = 0;
            error = false;
            isSameAngle = false;
            sameAngle = 0;
            coorIsOnline = false;
            //默认的超过该时间才保存日志
            saveTime = 5 * 60 ;
            //用于给zigbee协调器发送没有数据上传，可以上传一些控制请求信息
            noDataAck = new byte[9];
            //3个hash表，用于快速根据短地址，长地址来定位节点描述符的索引
            nodeMacIndexHash = new Dictionary<string, int>();
            nodeAddrIndexHash = new Dictionary<ushort, int>();
            nodeNameIndexHash = new Dictionary<string, int>();
            //待发送给zigbee网络的控制帧队列，所有的控制命令都需要先在这里排队等待空闲间隙
            SendPacket = new LinkedList<CtlPacket>();
            nodeInfoArray = new List<NodeDescribePacket>();
            nodeDataUsed = new NodeDescribePacket();
            nodeAck = new NodeDescribePacket();
            //启动多线程并发串口模块
           // StartSerialPort();
            //首先增加协调器节点的信息，防止丢失协调器信息后，其他节点上线后找不到父节点
            firstAddCoor();
            //初始化该帧
            initNoDataAck();

            //用于下面线程同步用
            resumeEvent = new ManualResetEvent(false);
            //建立循环发送线程，并且建立好后，线程阻塞等待用户控制
            sendThread = new Thread(sendMsgToZigbee);
            sendThread.Start();
            //启动软件系统的心跳
          //  timerOut.Enabled = true;
            timerOut.Interval = 1000;
            //Thread.Sleep(500);
           // timerOut.Start();
            try
            {
                initExcel(excelName + ".xls");
            }
            catch (Exception) { }

        }
        public void startCount()
        { timerOut.Start(); }
        private void initExcel(string name)
        {
            //为excel对象初始化
            //string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Data\data.xls";

            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Data\"+name;
            //Console.WriteLine("{0}--excel文件", tpath);
            if (!File.Exists(tpath))
            {
                MyExcelNamespace.MyExcel.CreateExcel(name);
            }
            // Console.WriteLine("{0}",tpath);
            try
            {
                excel = new MyExcelNamespace.MyExcel(tpath);
                excel.AddData<string>(1, 1, "数据列表");
                excel.SaveData();
            }
            catch (Exception)
            {
               // MessageBox.Show("请先关闭data.xls");
               // excel = new MyExcelNamespace.MyExcel(tpath);
            }
        }
        private void initNoDataAck()
        {
            noDataAck[0] = 0xfe;
            noDataAck[1] = 0x09;
            noDataAck[2] = 0x06;
            noDataAck[3] = 0x00;
            noDataAck[4] = 0x00;
            noDataAck[5] = 0x00;
            noDataAck[6] = 0x00;
            noDataAck[7] = 0x00;
            noDataAck[8] = 0xfb;
        }
        private void writeLineToText()
        {
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\line.txt";
            using (FileStream stream2 = File.Open(tpath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.SetLength(0);
                stream2.Close();
                using (StreamWriter sw = File.AppendText(tpath))
                {
                    sw.WriteLine(ecxelLine.ToString());
                    sw.WriteLine(excelName);
                    sw.Close();
                }

            }
        }
        private void firstAddCoor()
        {
            //预先增加协调器
            NodeDescribePacket newNode = new NodeDescribePacket();
            newNode.name = "V000";
            newNode.Mac = "0000000000000000";
            newNode.NetAddr = 0;
            nodeMacIndexHash.Add(newNode.Mac, 0);
            nodeNameIndexHash.Add(newNode.name, 0);
            nodeAddrIndexHash.Add(newNode.NetAddr, 0);
            nodeInfoArray.Add(newNode);
        }

        private void MainHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            //把ecxel行数写回文件
            writeLineToText();
            try
            {
                this.excel.RealeseResource();
            }
            catch (Exception) { }
            System.Environment.Exit(System.Environment.ExitCode);
            this.Dispose();
            this.Close();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //显示网络结构
            NetStruct net = new NetStruct(this);
            net.ShowDialog();
        }

        private void toolStrip1_Paint_1(object sender, PaintEventArgs e)
        {
            if (toolStrip1.RenderMode == ToolStripRenderMode.System)
            {
                Rectangle rect = new Rectangle(0, 0, toolStrip1.Width, toolStrip1.Height - 2);
                e.Graphics.SetClip(rect);
            }
        }
        #region 无误
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string name = ((LinkLabel)(sender)).Name.Trim();
            System.Diagnostics.Process.Start("notepad.exe", nodeInfoArray[nodeMacIndexHash[name]].myDataPath);
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Config config = new Config(this);
            config.ShowDialog();
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked)
            {
                checkBox14.Checked = false;
                checkBox13.Enabled = false;
                checkBox3.Checked = true; checkBox4.Checked = true; checkBox5.Checked = true; checkBox6.Checked = true; checkBox7.Checked = true;
                checkBox8.Checked = true; checkBox9.Checked = true; checkBox10.Checked = true; checkBox11.Checked = true; checkBox12.Checked = true;
            }
            else
            {
                checkBox13.Enabled = true;
                /*
               // checkBox14.Checked = true;
                //checkBox13.Checked = false;
                checkBox3.Checked = false; checkBox4.Checked = false; checkBox5.Checked = false; checkBox6.Checked = false; checkBox7.Checked = false;
                checkBox8.Checked = false; checkBox9.Checked = false; checkBox10.Checked = false; checkBox11.Checked = false; checkBox12.Checked = false;
              * */
            }
            //checkBox14.Checked = false;


        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox14.Checked)
            {
                checkBox13.Checked = false;
                checkBox14.Enabled = false;
                checkBox3.Checked = false; checkBox4.Checked = false; checkBox5.Checked = false; checkBox6.Checked = false; checkBox7.Checked = false;
            checkBox8.Checked = false; checkBox9.Checked = false; checkBox10.Checked = false; checkBox11.Checked = false; checkBox12.Checked = false;
            }
            else
            {
                checkBox14.Enabled = true;
                /*
                checkBox3.Checked = true; checkBox4.Checked = true; checkBox5.Checked = true; checkBox6.Checked = true; checkBox7.Checked = true;
                checkBox8.Checked = true; checkBox9.Checked = true; checkBox10.Checked = true; checkBox11.Checked = true; checkBox12.Checked = true;
               // checkBox13.Checked = true;*/
            }

        }


        #endregion

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox18.Checked)
            {
                checkBox16.Checked = false;
               // checkBox17.Checked = false;
            }
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox16.Checked)
            {
                checkBox18.Checked = false;
              //  checkBox17.Checked = false;
            }

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Help config = new Help();
            config.ShowDialog();
        }
        //updateLabel仅仅用于更新状态
        private void uptateCheckBox(int index)
        {
            if (nodeInfoArray[index].portChecked[0] != 0x02)
            {
                this.checkBox3.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0001) == 0x0000)
                    this.checkBox3.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox3.ForeColor = System.Drawing.Color.Red;
                if (nodeInfoArray[index].portChecked[0] == 0x01)
                    this.checkBox3.Checked = true;
                else
                    this.checkBox3.Checked = false;
            }
            else
                this.checkBox3.Enabled = false;

            if (nodeInfoArray[index].portChecked[1] != 0x02)
            {
                this.checkBox4.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0002) == 0x0000)
                    this.checkBox4.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox4.ForeColor = System.Drawing.Color.Red;
                if (nodeInfoArray[index].portChecked[1] == 0x01)
                    this.checkBox4.Checked = true;
                else
                    this.checkBox4.Checked = false;
            }
            else
                this.checkBox4.Enabled = false;

            if (nodeInfoArray[index].portChecked[2] != 0x02)
            {
                this.checkBox5.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0004) == 0x0000)
                    this.checkBox5.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox5.ForeColor = System.Drawing.Color.Red;

                if (nodeInfoArray[index].portChecked[2] == 0x01)
                    this.checkBox5.Checked = true;
                else
                    this.checkBox5.Checked = false;
            }
            else
                this.checkBox5.Enabled = false;

            if (nodeInfoArray[index].portChecked[3] != 0x02)
            {
                this.checkBox6.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0008) == 0x0000)
                    this.checkBox6.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox6.ForeColor = System.Drawing.Color.Red;

                if (nodeInfoArray[index].portChecked[3] == 0x01)
                    this.checkBox6.Checked = true;
                else
                    this.checkBox6.Checked = false;
            }
            else this.checkBox6.Enabled = false;

            if (nodeInfoArray[index].portChecked[4] != 0x02)
            {
                this.checkBox7.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0010) == 0x0000)
                    this.checkBox7.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox7.ForeColor = System.Drawing.Color.Red;

                if (nodeInfoArray[index].portChecked[4] == 0x01)
                    this.checkBox7.Checked = true;
                else
                    this.checkBox7.Checked = false;
            }
            else this.checkBox7.Enabled = false;

            if (nodeInfoArray[index].portChecked[5] != 0x02)
            {
                this.checkBox8.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0100) == 0x0000)
                    this.checkBox8.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox8.ForeColor = System.Drawing.Color.Red;

                if (nodeInfoArray[index].portChecked[5] == 0x01)
                    this.checkBox8.Checked = true;
                else
                    this.checkBox8.Checked = false;
            }
            else this.checkBox8.Enabled = false;
            if (nodeInfoArray[index].portChecked[6] != 0x02)
            {
                this.checkBox9.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0200) == 0x0000)
                    this.checkBox9.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox9.ForeColor = System.Drawing.Color.Red;
                if (nodeInfoArray[index].portChecked[6] == 0x01)
                    this.checkBox9.Checked = true;
                else
                    this.checkBox9.Checked = false;
            }
            else this.checkBox9.Enabled = false;
            if (nodeInfoArray[index].portChecked[7] != 0x02)
            {
                this.checkBox10.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0400) == 0x0000)
                    this.checkBox10.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox10.ForeColor = System.Drawing.Color.Red;
                if (nodeInfoArray[index].portChecked[7] == 0x01)
                    this.checkBox10.Checked = true;
                else
                    this.checkBox10.Checked = false;
            }
            else this.checkBox10.Enabled = false;
            if (nodeInfoArray[index].portChecked[8] != 0x02)
            {
                this.checkBox11.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0800) == 0x0000)
                    this.checkBox11.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox11.ForeColor = System.Drawing.Color.Red;

                if (nodeInfoArray[index].portChecked[8] == 0x01)
                    this.checkBox11.Checked = true;
                else
                    this.checkBox11.Checked = false;
            }
            else this.checkBox11.Enabled = false;
            if (nodeInfoArray[index].portChecked[9] != 0x02)
            {
                this.checkBox12.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x1000) == 0x0000)
                    this.checkBox12.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox12.ForeColor = System.Drawing.Color.Red;
                if (nodeInfoArray[index].portChecked[9] == 0x01)
                    this.checkBox12.Checked = true;
                else
                    this.checkBox12.Checked = false;
            }
            else this.checkBox12.Enabled = false;
#if false
            if (nodeInfoArray[index].port[0] != null)
            {
                this.checkBox3.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0001) == 0x0000)
                    this.checkBox3.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox3.ForeColor = System.Drawing.Color.Red;
            }
            else
                this.checkBox3.Enabled = false;

            if (nodeInfoArray[index].port[1] != null)
            {
                this.checkBox4.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0002) == 0x0000)
                    this.checkBox4.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox4.ForeColor = System.Drawing.Color.Red;
            }
            else
                this.checkBox4.Enabled = false;

            if (nodeInfoArray[index].port[2] != null)
            {
                this.checkBox5.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0004) == 0x0000)
                    this.checkBox5.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox5.ForeColor = System.Drawing.Color.Red;
            }
            else
                this.checkBox5.Enabled = false;

            if (nodeInfoArray[index].port[3] != null)
            {
                this.checkBox6.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0008) == 0x0000)
                    this.checkBox6.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox6.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox6.Enabled = false;

            if (nodeInfoArray[index].port[4] != null)
            {
                this.checkBox7.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0010) == 0x0000)
                    this.checkBox7.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox7.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox7.Enabled = false;

            if (nodeInfoArray[index].port[5] != null)
            {
                this.checkBox8.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0100) == 0x0000)
                    this.checkBox8.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox8.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox8.Enabled = false;
            if (nodeInfoArray[index].port[6] != null)
            {
                this.checkBox9.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0200) == 0x0000)
                    this.checkBox9.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox9.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox9.Enabled = false;
            if (nodeInfoArray[index].port[7] != null)
            {
                this.checkBox10.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0400) == 0x0000)
                    this.checkBox10.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox10.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox10.Enabled = false;
            if (nodeInfoArray[index].port[8] != null)
            {
                this.checkBox11.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x0800) == 0x0000)
                    this.checkBox11.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox11.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox11.Enabled = false;
            if (nodeInfoArray[index].port[9] != null)
            {
                this.checkBox12.Enabled = true;
                if ((nodeInfoArray[index].onFlag & 0x1000) == 0x0000)
                    this.checkBox12.ForeColor = System.Drawing.Color.Black;
                else
                    this.checkBox12.ForeColor = System.Drawing.Color.Red;
            }
            else this.checkBox12.Enabled = false;
#endif
        }

    }
}
