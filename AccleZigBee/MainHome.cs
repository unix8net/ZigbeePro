using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MyExcelNamespace;
namespace AccleZigBee
{
    public partial class MainHome : Form
    {
        public MainHome(System.IO.Ports.SerialPort port)
        {
            InitializeComponent();
            //软件系统串口模块
            serialPort = port;
            //初始化一些数据成员
            initial();
            //将循环发送线程挂起
            Pause();
        }

        #region 数据存放
        //MAC地址与数组索引之间的映射
        public Dictionary<string, int> nodeMacIndexHash;
        //节点地址与数组索引之间的映射
        public Dictionary<ushort, int> nodeAddrIndexHash;
        //节点名字与数组索引之间的映射
        public Dictionary<string, int> nodeNameIndexHash;
        //存放数组的描述符
        public List<NodeDescribePacket> nodeInfoArray;
        //避免处理收到数据时，每次都定义局部变量
        //作为接受数据用
        private NodeDescribePacket nodeDataUsed;
        //作为接收ACK时确实是哪一个加速度节点的ACK用
        private NodeDescribePacket nodeAck;

       // private string tmpAngleString;

        //count用于该系统的心跳，每一秒触发一次，也作为整个系统的软件时钟，所有时间都是从count中获取
        private UInt32 count;
        //超出该时间后，保存日志。用于剔除前边不稳定的数据
        private Int32 saveTime;

        //excel处理模块
        private MyExcel excel;
        //记录excel文件里已经存在的行数，为了每次重新启动软件后系统能够在文件末尾添加数据
        private int ecxelLine;
        private string excelName;
        #endregion

        private ToolTip tips;

        #region 算法标志
        //滤波
        public bool filter;
        //标定
        public bool modify;
        //记录数据
        public bool logData;
        //计算数据系数
        private  double accleAccuracy = Convert.ToDouble((1 << 24) - 1) / 10.0;
        #endregion


        #region 界面显示控制
        //记录当前打开的加速度节点，为的是更新波形时方便。
        //记录打开的end节点
        private int openingEndIndnex;
        //记录打开的路由器节点
        private int openingRouterIndex;
        //控制路由器节点显示的纵坐标
        private int routerCoorY = 90;

        //用于软件上显示时间时使用
        private int timeHours, timeMinutes, timeSeconds;
        //
        private int timeHoursInit, timeMinutesInit, timeSecondsInit;
        //软件启动后，界面上显示的灰色图标的标记，第一次使用后变失效
        private bool noDataWithGrayPic;
        public bool error;
        private bool coorIsOnline;
        public bool isSameAngle;
        private int sameAngle;
        #endregion

 #region 读取数据
        private bool ReadCalibrationCoefficient(NodeDescribePacket newNode)
        {
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\config.txt";
            if (!File.Exists(tpath))
            {
                return false;
            }
            else
            {
                using (StreamReader sr = new StreamReader(tpath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line == newNode.Mac)
                        {
                            line = sr.ReadLine();
                            string[] split = line.Split(new Char[] { ';' });
                            newNode.config[0] = System.Convert.ToDouble(split[0]);
                            newNode.config[1] = System.Convert.ToDouble(split[1]);
                            newNode.config[2] = System.Convert.ToDouble(split[2]);
                            newNode.config[3] = System.Convert.ToDouble(split[3]);
                            newNode.config[4] = System.Convert.ToDouble(split[4]);
                            sr.Close();
                            break;
                        }
                    }//while                       
                }//using
                return true;
            }
        }

        private bool ReadPortWithName(NodeDescribePacket newNode)
        {
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + newNode.name + @".txt";
            if (!File.Exists(tpath))
            {
                //MessageBox.Show("Config目录下的VXXX.txt未找到该网关节点的端口映射表，请在软件中配置端口映射并复位该网关节点即可");
                return false;
            }
            else
            {
                //读取端口映射值
                using (StreamReader sr = new StreamReader(tpath))
                {
                    string line;
                    if ((line = sr.ReadLine()) != null)
                    {
                        string[] split = line.Split(new Char[] { ';' });
                        int num = split.Length;
                        for (int j = 0; j < num; ++j)
                        {
                            //通过名字来获取MAC地址
                            //如果某个端口没有设备则设置成null
                            line = split[j].Trim();
                            if (line.Length != 0)
                            {
                                //port用于存放每个端口连接的加速度节点的硬件地址
                                newNode.port[j] = readMacByName(line);
                                //设置未被选中，但是已经使能，也即是该端口可以使用
                                newNode.portChecked[j] = 0x00;
                            }
                            else
                            {
                                //设置该端口，不能被使用
                                newNode.portChecked[j] = 0x02;
                                newNode.port[j] = null;
                            }
                            //  Console.WriteLine("当前{0}端口配置{1}", j, newNode.port[j]);
                        }
                        sr.Close();
                    }
                }//using
                return true;
            }
        }
#endregion

        public void LogError(string info)
        {
            string logPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Error.txt";
            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(info);
                sw.Close();
            }
        }
        //用于处理串口收到的所有数据
        public void DataProcess()
        {
            if (0x00 == tmpBuf[2])
            {
                #region 描述符

                byte nodeType = tmpBuf[3];
                ushort net = Convert.ToUInt16(tmpBuf[4] * 256 + tmpBuf[5]);
                string mac = Convert.ToString(tmpBuf[6], 16).PadLeft(2, '0') + Convert.ToString(tmpBuf[7], 16).PadLeft(2, '0')
                    + Convert.ToString(tmpBuf[8], 16).PadLeft(2, '0') + Convert.ToString(tmpBuf[9], 16).PadLeft(2, '0') +
                    Convert.ToString(tmpBuf[10], 16).PadLeft(2, '0') + Convert.ToString(tmpBuf[11], 16).PadLeft(2, '0') +
                    Convert.ToString(tmpBuf[12], 16).PadLeft(2, '0') + Convert.ToString(tmpBuf[13], 16).PadLeft(2, '0');
                mac = mac.ToUpper();
                ushort parentAddr = Convert.ToUInt16(tmpBuf[14] * 256 + tmpBuf[15]);
                //获取节点名字
                string name = readNameByMac(mac);
                if (name == null)
                {
                    //MessageBox.Show("该节点没有入库，请检查Config目录下的IEEE.txt。添加信息后复位该节点");
                  //  Console.WriteLine("name:{0},mac:{1}",name,mac);
                    Notify notify = new Notify("该节点没有入库，请检查Config目录下的IEEE.txt。添加信息后复位该节点");
                    notify.ShowDialog();
                    return;
                }
                //因为协调器在软件启动时已经虚拟出来，所以真正节点上线后需要特殊处理


                if (0x00 == nodeType)
                {
                    if (coorIsOnline)
                        return;
                    # region 协调器
                    if (nodeMacIndexHash.ContainsKey("0000000000000000"))
                    {
                        nodeMacIndexHash.Remove("0000000000000000");
                    }

                    if (!nodeMacIndexHash.ContainsKey(mac))
                        nodeMacIndexHash.Add(mac, 0);
                    else
                        nodeMacIndexHash[mac] = 0;

                    if (name != "V000")
                    {
                        nodeNameIndexHash.Remove("V000");
                        if (!nodeNameIndexHash.ContainsKey(name))
                            nodeNameIndexHash.Add(name, 0);
                        else
                            nodeNameIndexHash[name] = 0;
                    }
                    if (net != 0x0000)
                    {
                        nodeAddrIndexHash.Remove(0);
                        if (!nodeAddrIndexHash.ContainsKey(net))
                            nodeAddrIndexHash.Add(net, 0);
                        else
                            nodeAddrIndexHash[net] = 0;
                    }
                    nodeInfoArray[0].NodeName = nodeType;
                    nodeInfoArray[0].Mac = mac;
                    nodeInfoArray[0].NetAddr = net;
                    nodeInfoArray[0].name = name;
                    nodeInfoArray[0].ParentAddr = 0;
                    this.Invoke((EventHandler)(delegate
                    {
                        //按钮的名字就是MAC地址
                        nodeInfoArray[0].nodeBtn.Name = mac;
                        //按钮的文本就是通用名字
                        nodeInfoArray[0].nodeBtn.Text = name;
                        //修改协调器的节点图标属性
                        nodeInfoArray[0].nodeBtn.Image = global::AccleZigBee.Properties.Resources.Coor;
                        nodeInfoArray[0].nodeBtn.ToolTipText = mac;
                        nodeInfoArray[0].nodeBtn.Size = new System.Drawing.Size(56, 56);
                        // newNode.nodeBtn.Click += new System.EventHandler(this.toolStripSplitRouterClick);   
                       
                    })); 
                    coorIsOnline = true;
                    addStripBtn(mac);
                    #endregion
                }
                else
                {
                    if (!coorIsOnline)
                    {
                        this.Invoke((EventHandler)(delegate
                        {
                            //按钮的名字就是MAC地址
                            nodeInfoArray[0].nodeBtn.Name = "0000000000000000";
                            //按钮的文本就是通用名字
                            nodeInfoArray[0].nodeBtn.Text = "V000";
                            //修改协调器的节点图标属性
                            nodeInfoArray[0].nodeBtn.Image = global::AccleZigBee.Properties.Resources.Coor;
                            nodeInfoArray[0].nodeBtn.ToolTipText = "0000000000000000";
                            nodeInfoArray[0].nodeBtn.Size = new System.Drawing.Size(56, 56);
                            // newNode.nodeBtn.Click += new System.EventHandler(this.toolStripSplitRouterClick);   
                        }));
                        coorIsOnline = true;
 
                    }
                    //节点第一次上线
                    if (!nodeMacIndexHash.ContainsKey(mac))
                    {
                        NodeDescribePacket newNode = new NodeDescribePacket();
                        newNode.NodeName = nodeType;
                        newNode.Mac = mac;
                        newNode.NetAddr = net;
                        newNode.name = name;
                        newNode.ParentAddr = parentAddr;
                        //按钮的名字就是MAC地址
                        newNode.nodeBtn.Name = mac;
                        //按钮的文本就是通用名字
                        newNode.nodeBtn.Text = name;

                        //终端节点处理
                        if (0x02 == nodeType)
                        {
                            #region 终端节点
                            //[2]读取标定系数
                            if (!ReadCalibrationCoefficient(newNode))
                            {
                               // MessageBox.Show("没有找到config.txt标定文件,默认为各个系数为1");
                                Notify notify = new Notify("没有找到config.txt标定文件,默认为各个系数为1");
                                notify.ShowDialog();
                            }
                            //设置终端节点的图标与大小
                            this.Invoke((EventHandler)(delegate
                            {
                                newNode.nodeBtn.Image = global::AccleZigBee.Properties.Resources.End;
                                newNode.nodeBtn.ToolTipText = newNode.Mac;
                                newNode.nodeBtn.Size = new System.Drawing.Size(32, 32);
                                newNode.nodeBtn.Click += new System.EventHandler(this.toolStripSplitWaveClick);
                                newNode.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClicked);
                                //设置linkLabel,为了防止与toolstripbtn同名，在后边加了空格
                                newNode.linkLabel.Name = newNode.Mac + " ";
                             }));
                            newNode.count = count;
                            #endregion

                        }//终端处理完毕
                        else if (0x01 == nodeType)
                        {
                            #region 路由器
                            //设置路由器节点的图标与大小
                            this.Invoke((EventHandler)(delegate
                            {
                            newNode.nodeBtn.Image = global::AccleZigBee.Properties.Resources.midRouter;
                            newNode.nodeBtn.ToolTipText = newNode.Mac;
                            newNode.nodeBtn.Size = new System.Drawing.Size(56, 56);
                            newNode.nodeBtn.Click += new System.EventHandler(this.toolStripSplitRouterClick);
                            }));
                            if (!ReadPortWithName(newNode))
                            {
                               // MessageBox.Show("未找到该开关节点的端口映射表，请重新配置");
                                Notify notify = new Notify("未找到该开关节点的端口映射表，请重新配置");
                                notify.ShowDialog();
                                return;
                            }
                            #endregion
                        }

                        nodeInfoArray.Add(newNode);
                        if(!nodeMacIndexHash.ContainsKey(mac))
                            nodeMacIndexHash.Add(mac, nodeInfoArray.Count - 1);
                        if (!nodeNameIndexHash.ContainsKey(name))
                            nodeNameIndexHash.Add(name, nodeInfoArray.Count - 1);
                        else
                            nodeNameIndexHash[name] = nodeInfoArray.Count - 1;

                        //更新短地址映射的数组索引
                        if (nodeAddrIndexHash.ContainsKey(net))
                            nodeAddrIndexHash[net] = nodeInfoArray.Count - 1;
                        else
                            nodeAddrIndexHash.Add(net, nodeInfoArray.Count - 1);
                        if (newNode.NodeName != 0x02)
                            addStripBtn(newNode.Mac);
                        this.toolStripStatusLabel3.Text = "节点数:" + nodeInfoArray.Count.ToString();
                      //  Console.WriteLine("节点{0},的索引是{1}", mac, nodeInfoArray.Count - 1);
                    }//如果是节点第一次上线
                    else
                    {
                        int index = nodeInfoArray.Count;
                        try
                        {
                            index = nodeMacIndexHash[mac];
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        if (index >= nodeInfoArray.Count)
                        {
                            return;
                        }
                        try
                        {   
                            if (net != nodeInfoArray[index].NetAddr)
                            {
                                int outIndex;
                                if (nodeAddrIndexHash.TryGetValue(nodeInfoArray[index].NetAddr, out outIndex))
                                {
                                    nodeAddrIndexHash.Remove(nodeInfoArray[index].NetAddr);
                                    nodeAddrIndexHash.Add(net, index);
                                }
                            }
                        }catch(Exception)
                        {
                            if (nodeAddrIndexHash.ContainsKey(net))
                            {
                                nodeAddrIndexHash[net] = index;
                            }
                            else
                                nodeAddrIndexHash.Add(net, index);
                        }

                       // Console.WriteLine("再次上线节点的MAC地址为{0},IP地址为{1}",mac, net);
                        nodeInfoArray[index].NetAddr = net;
                        nodeInfoArray[index].NodeName = nodeType;
                        nodeInfoArray[index].Mac = mac;
                        nodeInfoArray[index].name = name;
                        nodeInfoArray[index].ParentAddr = parentAddr;
                        //foreach(KeyValuePair<ushort,int>kvp in nodeAddrIndexHash)
                        //{
                        //}

                        this.Invoke((EventHandler)(delegate
                        {
                                //按钮的名字就是MAC地址
                                nodeInfoArray[index].nodeBtn.Name = mac;
                                //按钮的文本就是通用名字
                                nodeInfoArray[index].nodeBtn.Text = name;
                                nodeInfoArray[index].nodeBtn.ToolTipText = mac;
                        }));
                        if (0x01 == nodeType)
                        {
                            
                            if (!ReadPortWithName(nodeInfoArray[index]))
                            {
                                //MessageBox.Show("未找到该开关节点的端口映射表，请重新配置");
                                Notify notify = new Notify("未找到该开关节点的端口映射表，请重新配置");
                                notify.ShowDialog();
                                return;
                            }
                        }
                        else
                        {
                            nodeInfoArray[index].avg = 0;
                            nodeInfoArray[index].count = count;
                            nodeInfoArray[index].sum = 0;
                            nodeInfoArray[index].dataSaveCount = 1;
                            nodeInfoArray[index].head = 0;
                            nodeInfoArray[index].tail = 0;
                            nodeInfoArray[index].flag = false;
                            nodeInfoArray[index].packetCount = 0;
                            //nodeInfoArray[index].packetCount = 0;
                            if (!ReadCalibrationCoefficient(nodeInfoArray[index]))
                            {
                                //MessageBox.Show("没有找到config.txt标定文件,默认为各个系数为1");
                                Notify notify = new Notify("没有找到config.txt标定文件,默认为各个系数为1");
                                notify.ShowDialog();
                            }
                        }
                    }
                }
                #endregion
            }//if (0x00 == tmpBuf[2])
            else if (0x01 == tmpBuf[2])
            {
                #region 数据处理
                int k = 6;
                double data3;
                int index = -1;
# if false
                try
                {
                  
                    index = nodeAddrIndexHash[Convert.ToUInt16(tmpBuf[3] * 256 + tmpBuf[4])];
                    //增加该节点收到包的计数
                    ++nodeInfoArray[index].packetCount;
                    nodeDataUsed = nodeInfoArray[index];
                    
                //    Console.WriteLine("收到MAC地址{0}的数据，他的IP地址是{1},索引是{2}",nodeDataUsed.Mac,tmp,index);
                }
                catch (Exception)
                {
                    int tmp = Convert.ToUInt16(tmpBuf[3] * 256 + tmpBuf[4]);
                    Console.WriteLine("错误的匹配，当前IP为{0}",tmp);
                    if (!error)
                    {
                        //MessageBox.Show("收到未知节点的数据，请检查端口映射是否添加正确，设备是否入库");
                        Notify notify = new Notify("没收到未知节点的数据，请检查端口映射是否添加正确，设备是否入库");
                        notify.ShowDialog();
                        LogError(tmpBuf.ToString()+" "+index.ToString()+" ");
                        error = true;
                    }
                    return;
                }
#else
                if (nodeAddrIndexHash.TryGetValue(Convert.ToUInt16(tmpBuf[3] * 256 + tmpBuf[4]), out index))
                {
                    //增加该节点收到包的计数
                    ++nodeInfoArray[index].packetCount;
                    nodeDataUsed = nodeInfoArray[index];
                }
                else
                {
                    //int tmp = Convert.ToUInt16(tmpBuf[3] * 256 + tmpBuf[4]);///////////////////////
                    if (!error)//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    {
                        //MessageBox.Show("收到未知节点的数据，请检查端口映射是否添加正确，设备是否入库");
                        Notify notify = new Notify("没收到未知节点的数据，请检查端口映射是否添加正确，设备是否入库");
                        notify.ShowDialog();
                      //  Console.WriteLine("数据异常：地址:{0},{1},{2}",tmp,tmpBuf[3],tmpBuf[4]);
                       // LogError(tmpBuf.ToString() + " " + index.ToString() + " ");
                        error = true;
                    }
                    return;
                }


#endif
                //开始处理数据
                if (0x20 == tmpBuf[k])
                {
                    Int32 tmpData;
                    tmpBuf[k + 1] ^= 0x80;
                    if ((tmpBuf[k + 1] & 0x80) != 0) //负数
                        tmpData = (65536 * tmpBuf[k + 1] + 256 * tmpBuf[k + 2] + tmpBuf[k + 3]) - 16777216;
                    else
                        tmpData = 65536 * tmpBuf[k + 1] + 256 * tmpBuf[k + 2] + tmpBuf[k + 3];
                    data3 = Convert.ToDouble(tmpData) / accleAccuracy;
                    //将数据放到循环队列
                    nodeDataUsed.inptData(data3); //元素数据

                    if (nodeDataUsed.packetCount == UInt32.MaxValue)
                        nodeDataUsed.packetCount = 2;

                    //节点收到第一次数据，才建立文件.系统将excel行的创建与文件的创建延迟到了这里
                    if (nodeDataUsed.packetCount == 1)
                    {
                        //当前需要写的行号
                        nodeInfoArray[index].line = nodeDataUsed.line = ++ecxelLine;
                        /***这里一定要先创建myDataPath,因为creatMyDataText中会对角度文件进行处理**/
                        //创建原始数据文件
                        nodeDataUsed.myDataPath = nodeInfoArray[index].myDataPath = creatMyDataText(nodeDataUsed.name);
                       // Console.WriteLine("名字是446----{0}",nodeDataUsed.name);
                        Thread.Sleep(500);
                        //创建excel行+++++++++++++++++++++++++++++++++++++++++++++
                        nodeDataUsed.path = nodeInfoArray[index].path = creatDataTxt(nodeDataUsed.name);
                        Thread.Sleep(500);
                        using (StreamWriter sw = File.AppendText(nodeDataUsed.myDataPath))
                        {
                            sw.WriteLine(data3.ToString());
                            sw.Close();
                        }

                    }



                    //为了减少数据量，每4个数据才记录一次
                    if ((nodeDataUsed.packetCount & 1) == 0)
                    {
                        //偶数折半后还是偶数，则表示为4的倍数
                        if (((nodeDataUsed.packetCount >> 4) & 1) == 0)
                        {
                            using (StreamWriter sw = File.AppendText(nodeDataUsed.myDataPath))
                            {
                                sw.WriteLine(data3.ToString());
                                sw.Close();
                            }
                        }
                    } 



                    if (filter)
                    {
                        //如果未满30组数据
                        if (nodeDataUsed.flag == false)
                        {
                            //小于26个数的均值
                            data3 = nodeDataUsed.sum / (nodeDataUsed.tail - nodeDataUsed.head);
                        }
                        else
                        {
                           // data3 = nodeDataUsed.sum / 26;
                            //等于26个值得均值
                            data3 = nodeDataUsed.sum * 0.03846;
                        }
                    }
                    //如果选择了标定
                    if (modify)
                    {
                        double tmpdata3 = data3;
                        double retdata3 = nodeDataUsed.config[4];
                        retdata3 += nodeDataUsed.config[3] * tmpdata3; tmpdata3 *= data3;
                        retdata3 += nodeDataUsed.config[2] * tmpdata3; tmpdata3 *= data3;
                        retdata3 += nodeDataUsed.config[1] * tmpdata3; tmpdata3 *= data3;
                        retdata3 += nodeDataUsed.config[0] * tmpdata3;
                        data3 = retdata3;
                        //标定后计算完成
                    }



                    //是否更新label实时显示数据
                    if (nodeDataUsed.dataLabel.Visible)
                    {
                        this.Invoke((EventHandler)(delegate
                        {
                            nodeDataUsed.dataLabel.Text = Math.Round(data3, 4).ToString();
                        }));
                    }


                    //更新波形显示
                    //如果当前收到的数据的索引是打开的索引
                    if ((index == openingEndIndnex) && this.userControl11.Visible)
                    {
                        this.Invoke((EventHandler)(delegate { update(index); }));
                    }
                    
                    //检查数据超时保存时间到
                    if (nodeDataUsed.count > count)
                    {
                        if (4294967290 - nodeDataUsed.count + count < saveTime)
                        {
                            nodeDataUsed.avg = data3;
                            return;
                        }
                    }
                    else
                    {
                        if (count - nodeDataUsed.count < saveTime)
                        {
                            nodeDataUsed.avg = data3;
                            return;
                        }
                    }
                    
                    //求均值
                    data3 = (data3 + nodeDataUsed.avg) * 0.5;
                    //保存该节点的均值
                    nodeDataUsed.avg = data3;

                    if (nodeDataUsed.dataSaveCount == 1)
                    {
                        //注释原因：以前为增加数据到记事本，现在导入到excel
#if false
                        using (StreamReader sr = new StreamReader(nodeDataUsed.path))
                        {
                            tmpAngleString = sr.ReadLine();
                            sr.Close();
                        }
                        using (StreamWriter sw = new StreamWriter(nodeDataUsed.path, false))
                        {

                            sw.WriteLine(tmpAngleString);
                            sw.WriteLine(data3.ToString());
                            sw.Close();
                        } 
#endif
                        try
                        {
                            //防止excel在换文件时，导致数据错乱
                            if (nodeDataUsed.line <= ecxelLine)
                            {
                                excel.AddData<string>(nodeDataUsed.line, 4, nodeDataUsed.avg.ToString());
                                excel.SaveData();
                            }
                        }
                        catch (Exception)
                        {
                            Notify notify = new Notify("请关闭data.xls,在软件中使用转存后打开data_copy.xls,软件稍后需要使用data.xls");
                            notify.ShowDialog();                  
                            try
                            {
                                excel.RealeseResource();
                                initExcel(excelName+".xls");
                                excel.AddData<string>(nodeDataUsed.line, 4, nodeDataUsed.avg.ToString());
                                excel.SaveData();
                            }
                            catch (Exception)
                            {
                            }
                        }
                        ++nodeDataUsed.dataSaveCount;
                    }
                    else if (nodeDataUsed.dataSaveCount >= 50)
                        nodeDataUsed.dataSaveCount = 1;
                    else
                        ++nodeDataUsed.dataSaveCount;
#if false
                    Console.WriteLine("{0}",nodeDataUsed.dataSaveCount);
                    using (FileStream stream2 = File.Open(nodeDataUsed.path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream2.Seek(0, SeekOrigin.Begin);
                        stream2.SetLength(0);
                        stream2.Close();
                        using (StreamWriter sw = File.AppendText(nodeDataUsed.path))
                        {
                            sw.WriteLine(data3.ToString());
                            sw.Close();
                        }
                        
                    }
#endif

                }//if (0x20 == tmpBuf[k])加速度数据处理完毕
                #endregion
            }//数据处理完毕
            else if (0x03 == tmpBuf[2])
            {
                #region 控制状态返回
                int index = nodeAddrIndexHash[Convert.ToUInt16(tmpBuf[3] * 256 + tmpBuf[4])];
                nodeAck = nodeInfoArray[index];
                //更新开关状态
                nodeInfoArray[index].onFlag = Convert.ToUInt16(tmpBuf[5] * 256 + tmpBuf[6]);
                if(index == openingRouterIndex)
                {
                    uptateCheckBox(index);
                }
                #endregion 
            }
            else if (0x04 == tmpBuf[2])
            {
                #region  有空隙时间
                Monitor.Enter(this.ls);
                if (SendPacket.Count != 0)
                {
                    //发送数据，并返回ACK值
                    ACK = SendPacket.First.Value.doTask();
                    this.toolStripStatusLabel2.Text = "当前正在尝试发送编号为"+ACK.ToString()+"的控制命令";
                }
                else
                {
                    //this.toolStripStatusLabel2.Text = "正确收到ACK编号" + ACK.ToString() + "/当前共" + SendPacket.Count.ToString() + "个命令在排队"; ;
                    serialPort.Write(noDataAck, 0, 9);
                }
                Monitor.Exit(this.ls);
                #endregion
            }
            else if (0x07 == tmpBuf[2])
            {
                #region ACK返回
                if (tmpBuf[3] == ACK)
                {
                    // Console.WriteLine("队首元素发出后ACK收到，当前正在排队的命令共{0}", SendPacket.Count);
                    if (SendPacket.Count != 0)
                    {
                        if (SendPacket.Count != 0)
                        SendPacket.RemoveFirst();
                        this.toolStripStatusLabel2.Text = "正确收到ACK编号" + ACK.ToString() + "/当前共" + SendPacket.Count.ToString() + "个命令在排队";
                    }
                }
                else
                {
                    if(SendPacket.Count!=0)
                        SendPacket.RemoveLast();
                    Notify notify = new Notify("ACK返回错误,设备间同步出错，正在重新协调同步");
                    notify.ShowDialog();
                    //MessageBox.Show("ACK返回错误,设备间同步出错");
                }
                #endregion
            }
        }
        public void clearPacketCnt(string name)
        {
            try
            {
                if (nodeNameIndexHash.ContainsKey(name))
                {
                    nodeInfoArray[nodeNameIndexHash[name]].packetCount = 0;
                }
            }
            catch (Exception) { }
        }
        //增加协调器或者网关（路由器）图标，与旁边的checkbox图标
        public  void addStripBtn(string key)
        {
            int index;
            index = nodeMacIndexHash[key];
            //key为唯一的MAC地址，addr则是临时地址，可以更新图标
            this.Invoke((EventHandler)(delegate
            {
                if (!noDataWithGrayPic)
                {
                    toolStripNode.Items.Remove(toolStripButton6);
                    toolStripNode.Items.Remove(toolStripButton7);
                    noDataWithGrayPic = true;
                    this.toolStripButton6.Dispose();
                    this.toolStripButton7.Dispose();
                    this.toolStripNode.Items.Add(nodeInfoArray[0].nodeBtn);
                   // routerCoorY += 60;
                    if (index == 0)
                        return;
                }
                
                toolStripStatusLabel1.Text = "网关列表中增加名为:" + nodeInfoArray[index].name+"的节点";
                try
                {
                    //由MAC地址，得到数组索引。由数组索引来获取nodeBtn
                    this.toolStripNode.Items.Add(nodeInfoArray[index].nodeBtn);
                }
                catch (Exception) { }
                nodeInfoArray[index].checkBox.Location = new System.Drawing.Point(110, routerCoorY);
                //记录下checkBox的位置
                nodeInfoArray[index].cx = 100;
                nodeInfoArray[index].cy = routerCoorY;
                routerCoorY += 60;
                //给网关节点右边增加checkBox
                if (nodeInfoArray[index].NodeName == 0x01)
                    this.panelNode.Controls.Add(nodeInfoArray[index].checkBox);
            }));
        }

        //波形显示控制
        private void toolStripSplitWaveClick(object sender, EventArgs e)
        {
            openingEndIndnex = nodeMacIndexHash[((ToolStripButton)sender).Name];
            int x = this.groupBox2.Location.X + 40;
            int y = this.groupBox2.Location.Y + nodeInfoArray[openingEndIndnex].cy + 133;
            //计算自定义控件的坐标
            this.userControl11.Location = new System.Drawing.Point(x, y);
            this.userControl11.Visible = true;
            //以上为显示控件，显示完成现在更新数据
            update(openingEndIndnex);
        }
        private void update(int index)
        {
            NodeDescribePacket tmpNode = nodeInfoArray[index];
            int x, y, n = 1;
            if (tmpNode.flag) //已经收集到26条数据
                n = 26;
            else
                n = tmpNode.tail - tmpNode.head;
            int s = tmpNode.head;
            List<int> px = new List<int>();
            List<int> py = new List<int>();
            for (int i = 0; i < n; i++)
            {

                if (tmpNode.data[s] > 0.0)
                {
                    x = i * 20;
                    y = (int)(tmpNode.data[s] * 1250); //<50
                    y = 80 - y;
                    px.Add(x);
                    py.Add(y);
                      //Console.WriteLine("+数据为{0}，x={1},y={2}", tmpNode.data[s], x, y);
                }
                else
                {
                    x = i * 20;
                    y = 80 + (int)(-tmpNode.data[s] *1250);
                    px.Add(x);
                    py.Add(y);
                       //  Console.WriteLine("-数据为{0}，x={1},y={2}", tmpNode.data[s], x, y);
                }
                s = (s + 1) % 26;
            }
            this.userControl11.startDisplay(px, py);
        }



        //先控制路由器或者协调器，后再进行加速度节点控制
        private void toolStripSplitRouterClick(object sender, EventArgs e)
        {

            int localX= 70, localY = 20;
            System.Windows.Forms.ToolStripButton toolStripNodeButton = (System.Windows.Forms.ToolStripButton)(sender);     
            int index  = nodeMacIndexHash[toolStripNodeButton.Name];
            this.toolStripStatusLabel1.Text = "当前选中节点为"+nodeInfoArray[index].name;
            openingRouterIndex = index;
            //对选中标志这个图片进行移动
            isCheckPictureBox.Location = new System.Drawing.Point(nodeInfoArray[index].cx + 50,nodeInfoArray[index].cy );
           // Console.WriteLine();
            //清理所有toolstrip的图标，接下来还清理绝大部分label
            toolStripEnd.Items.Clear();
            while (true)
            {
                foreach (Control i in this.panelAccleNode.Controls)
                {
                    if (i.Name == "label2")
                        continue;
                    else if (i.Name == "toolStripEnd")
                        continue;
                    else
                        this.panelAccleNode.Controls.Remove(i);
                }
                if (this.panelAccleNode.Controls.Count == 2)
                    break;
            }
            //接下来是控制加速度节点的显示
#if false
            ushort addr =  nodeInfoArray[index].NetAddr;
            foreach (NodeDescribePacket node in nodeInfoArray)
            {
                if ((node.NodeName == 0x02) && (node.ParentAddr == addr))
                {
                    this.toolStripEnd.Items.Add(node.nodeBtn);
                    node.dataLabel.Location = new System.Drawing.Point(localX, localY-1);
                    node.cx = localX;
                    node.cy = localY-2;
                    node.dataLabel.Text =  Math.Round(node.data[node.tail], 4).ToString();
                    node.linkLabel.Location = new System.Drawing.Point(localX+70, localY-1);
                    localY += 39;

                    this.panelAccleNode.Controls.Add(node.dataLabel);
                    this.panelAccleNode.Controls.Add(node.linkLabel);
                }
            }
#else
            int tryGetIndex;
            //循环该协调器或者路由器端口映射
            int j = 0;
            foreach (string name in nodeInfoArray[index].port)
            {
                j++;
                if (name == null)
                    continue;
                //如果该节点已经上线才能显示
                if (nodeMacIndexHash.TryGetValue(name, out tryGetIndex))
                {
                    if (nodeInfoArray[tryGetIndex].NodeName != 0x02)
                        return;
                    this.toolStripEnd.Items.Add(nodeInfoArray[tryGetIndex].nodeBtn);
                    nodeInfoArray[tryGetIndex].dataLabel.Location = new System.Drawing.Point(localX, localY - 1);
                    nodeInfoArray[tryGetIndex].cx = localX;
                    nodeInfoArray[tryGetIndex].cy = localY - 2;
                    nodeInfoArray[tryGetIndex].dataLabel.Text = Math.Round(nodeInfoArray[tryGetIndex].data[nodeInfoArray[tryGetIndex].tail], 4).ToString();
                    nodeInfoArray[tryGetIndex].linkLabel.Location = new System.Drawing.Point(localX + 80, localY - 1);
                    nodeInfoArray[tryGetIndex].linkLabel.Text = "端口" + j.ToString();
                    localY += 39;
                    this.panelAccleNode.Controls.Add(nodeInfoArray[tryGetIndex].dataLabel);
                    this.panelAccleNode.Controls.Add(nodeInfoArray[tryGetIndex].linkLabel);
                }
            }
#endif
            if (!(this.isCheckPictureBox.Visible))
                this.isCheckPictureBox.Visible = true;
            //更新端口开闭状态与是否可用状态
            uptateCheckBox(index);
        }

        //周期发送
        private void cycleSend()
        {
            int n = nodeInfoArray.Count;
            UInt16 ctlWord = 0x0000;
            //首选判断命令

            for (int i = 0; i < n; ++i)
            {
                if (nodeInfoArray[i].NodeName == 0x02) continue;
                if (nodeInfoArray[i].checkBox.Checked == false) continue;

                ctlWord = 0x0000;
                if (nodeInfoArray[i].portChecked[0] == 0x01)
                    ctlWord |= 0x0001;
                if (nodeInfoArray[i].portChecked[1] == 0x01)
                    ctlWord |= 0x0002;
                if (nodeInfoArray[i].portChecked[2] == 0x01)
                    ctlWord |= 0x0004;
                if (nodeInfoArray[i].portChecked[3] == 0x01)
                    ctlWord |= 0x0008;
                if (nodeInfoArray[i].portChecked[4] == 0x01)
                    ctlWord |= 0x0010;
                if (nodeInfoArray[i].portChecked[5] == 0x01)
                    ctlWord |= 0x0100;
                if (nodeInfoArray[i].portChecked[6] == 0x01)
                    ctlWord |= 0x0200;
                if (nodeInfoArray[i].portChecked[7] == 0x01)
                    ctlWord |= 0x0400;
                if (nodeInfoArray[i].portChecked[8] == 0x01)
                    ctlWord |= 0x0800;
                if (nodeInfoArray[i].portChecked[9] == 0x01)
                    ctlWord |= 0x1000;
#if false
                if (nodeInfoArray[i].status == 0x00)
                //2013-08-11修改
#endif          
                if(isFirstCycleFlag)
                {
                   // isFirstCycleFlag = false;
                    nodeInfoArray[i].status = 0x01;
                    //没有被选中的端口保持原状，选中的端口打开
                    ctlWord |= nodeInfoArray[i].onFlag;
                    this.pictureBox1.Image = global::AccleZigBee.Properties.Resources.on1;
                    toolStripStatusLabel1.Text = "当前正在给" + nodeInfoArray[i].name+"的所选端口发送打开命令";
                }
                else
                {
                  //  isFirstCycleFlag = true;
                    //没有被选中的保持原样，选中的端口关闭
                    ctlWord = (UInt16)((~ctlWord) & nodeInfoArray[i].onFlag);
                    this.pictureBox1.Image = global::AccleZigBee.Properties.Resources.off2;
                    toolStripStatusLabel1.Text = "当前正在给" + nodeInfoArray[i].name + "的所选端口发送关闭命令";
                    nodeInfoArray[i].status = 0x00;
                }
                timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                sendMsg(i, ctlWord);
                Thread.Sleep(500);
            }//for 节点循环
            isFirstCycleFlag = !isFirstCycleFlag;
        }

        //获取开关标志,用于手动控制时获取每个端口的状态并改变
        private UInt16 getOnFlag(int index, bool onOrOff)
        {
            //获取上一个端口打开表
            UInt16 ctlWord = nodeInfoArray[index].onFlag;
            if ((nodeInfoArray[index].portChecked[0]==0x01) && (nodeInfoArray[index].port[0] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0001;
                else
                    ctlWord &= 0xfffe;
            }
            if ((nodeInfoArray[index].portChecked[1] == 0x01) && (nodeInfoArray[index].port[1] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0002;
                else
                    ctlWord &= 0xfffd;
            }
            if ((nodeInfoArray[index].portChecked[2]==0x01) && (nodeInfoArray[index].port[2] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0004;
                else
                    ctlWord &= 0xfffb;
            }
            if ((nodeInfoArray[index].portChecked[3]==0x01) && (nodeInfoArray[index].port[3] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0008;
                else
                    ctlWord &= 0xfff7;
            }
            if ((nodeInfoArray[index].portChecked[4]==0x01) && (nodeInfoArray[index].port[4] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0010;
                else
                    ctlWord &= 0xffef;
 
            }
            if ((nodeInfoArray[index].portChecked[5]==0x01) && (nodeInfoArray[index].port[5] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0100;
                else
                    ctlWord &= 0xfeff;
            }
            if ((nodeInfoArray[index].portChecked[6]==0x01) && (nodeInfoArray[index].port[6] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0200;
                else
                    ctlWord &= 0xfdff;
            }
            if ((nodeInfoArray[index].portChecked[7]==0x01) && (nodeInfoArray[index].port[7] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0400;
                else
                    ctlWord &= 0xfbff;
 
            }
            if ((nodeInfoArray[index].portChecked[8]==0x01) && (nodeInfoArray[index].port[8] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x0800;
                else
                    ctlWord &= 0xf7ff;
            }

            if ((nodeInfoArray[index].portChecked[9]==0x01) && (nodeInfoArray[index].port[9] != null))
            {
                if (onOrOff)
                    ctlWord |= 0x1000;
                else
                    ctlWord &= 0xefff;
            }
            nodeInfoArray[index].onFlag = ctlWord;
            return ctlWord;
        }
        //周期时间到达，继续执行。  或者点击button事件
        private void button1_Click(object sender, EventArgs e)
        {  
            //让循环控制停下来
            Pause();
            //让自动发送停止
            timerSend.Stop();
            if (!checkOk())
            {
                Notify notify = new Notify("没有选中开关设备或没有选中端口");
                notify.ShowDialog();
                //MessageBox.Show("没有选中开关设备或没有选中端口");
                return;
            }
            int n = nodeInfoArray.Count;
            UInt16 ctlWord = 0x0000;
            string cmd="";
            //首选判断命令
            if (comboBox10.Text.Trim().Length == 0)
            {
                saveTime = 5 * 60;
                //MessageBox.Show("数据保存时间输入错误，自动默认为5分钟");
                Notify notify = new Notify("数据保存时间输入错误，自动默认为5分钟");
                notify.ShowDialog();
            }
            else
                saveTime = Int32.Parse(comboBox10.Text) * 60;
            for (int i = 0; i < n; ++i)
            {
                if (nodeInfoArray[i].NodeName == 0x02) continue;
                //如果该设备被选中
                if (nodeInfoArray[i].checkBox.Checked == false) continue;
                //nodeInfoArray[i].nodeBtn.PerformClick();
                if (checkBox18.Checked)
                {
                    this.pictureBox1.Image = global::AccleZigBee.Properties.Resources.on1;
                    ctlWord = getOnFlag(i, true);
                    nodeInfoArray[i].status = 0x01;
                    cmd = "打开";
                    oneFlag = true;
                    label3.Text = "运行时间:";
                    timeHours = 0;
                    timeMinutes = 0;
                    timeSeconds = 0;
                }
                if (checkBox16.Checked)
                {
                     this.pictureBox1.Image = global::AccleZigBee.Properties.Resources.off2;
                     ctlWord = getOnFlag(i, false);
                    nodeInfoArray[i].status = 0x00;
                    cmd = "关闭";
                    oneFlag = false;
                    label3.Text = "剩余时间:";
                    hourLabel.Text = "0";
                    minuteLabel.Text = "0";
                    secondsLabel.Text = "0";

                }
                toolStripStatusLabel1.Text = "当前正在" + cmd + "节点" + nodeInfoArray[i].name + "的所有已经选中端口";
                sendMsg(i,ctlWord);
                Thread.Sleep(100);
            }//for 节点循环
            toolStripStatusLabel1.Text = "单次控制命令发送完毕!";
        }

        private void clearTimeCount()
        {
            timeHours = 0;
            timeMinutes = 0;
            timeSeconds = 0;
            this.hourLabel.Text = "0";
            this.minuteLabel.Text = "0";
            this.secondsLabel.Text = "0";
        }
        //给所有开关节点进行端口循环，循环结束整个板子都启动工作
        private void sendMsgToZigbee()
        {
            int i,n;
            int cycleTime = 10;
            while (true)
            {    
                if (paused)
                {
                    resumeEvent.WaitOne();
                }
                n = nodeInfoArray.Count;
                #region 根据端口发送

                for (i = 0; i < n; ++i)
                {
                    if (nodeInfoArray[i].NodeName == 0x02) continue;
                    if (nodeInfoArray[i].checkBox.Checked == false) continue;
                    this.Invoke((EventHandler)(delegate
                    {
                        cycleTime = Int32.Parse(comboBox3.Text) * 1000 + Int32.Parse(comboBox2.Text) * 1000 * 60 + Int32.Parse(comboBox1.Text) * 3600 * 1000;
                    }));
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    this.Invoke((EventHandler)(delegate
                    {
                        nodeInfoArray[i].nodeBtn.PerformClick();
                    }));
                    if (nodeInfoArray[i].portChecked[0] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到"+nodeInfoArray[i].name+"/端口号:1";
                        sendMsg(i, 0x0001);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }

                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[1] == 0x01)
                    {

                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:2";
                        sendMsg(i, 0x0002);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[2] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:3";
                        sendMsg(i, 0x0004);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[3] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:4";
                        sendMsg(i, 0x0008);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[4] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:5";
                        sendMsg(i, 0x0010);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[5] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:6";
                        sendMsg(i, 0x0100);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[6] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:7";
                        sendMsg(i, 0x0200);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[7] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:8";
                        sendMsg(i, 0x0400);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[8] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:9";
                        sendMsg(i, 0x0800);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    if (paused)
                    {
                        resumeEvent.WaitOne();
                        break;
                    }
                    if (nodeInfoArray[i].portChecked[9] == 0x01)
                    {
                        toolStripStatusLabel1.Text = "当前正循环到" + nodeInfoArray[i].name + "/端口号:10";
                        sendMsg(i, 0x1000);
                        timeHours = timeHoursInit; timeMinutes = timeMinutesInit; timeSeconds = timeSecondsInit;
                        Thread.Sleep(cycleTime + 1000);
                    }
                    sendMsg(i, 0x0000);

                }
                #endregion
            }
         
        }

        void sendMsg(int i, UInt16 ctlWord)
        {
            cnt++;
            if (cnt == 250)
                cnt = 0;
            CtlPacket ctlPacket = new CtlPacket(serialPort, nodeInfoArray[i].NetAddr, ctlWord, cnt);
            Monitor.Enter(this.ls);
            SendPacket.AddLast(ctlPacket);
          //  this.Invoke((EventHandler)(delegate
           // {
                this.toolStripStatusLabel2.Text = "当前共" + SendPacket.Count.ToString() + "个命令在排队";
           // }));
           // Console.WriteLine("当前正在排队的命令为{0}",SendPacket.Count);
            Monitor.Exit(this.ls);
        }
        private void timerSend_Tick(object sender, EventArgs e)
        {
            cycleSend();
        }
        private bool checkOk()
        {
            int n = nodeInfoArray.Count;
            int i,j;
            
            for (i = 0; i < n; ++i)
            {
               // Console.WriteLine("mainhome----1171----{0}--{1}", nodeInfoArray[i].name, nodeInfoArray[i].checkBox.Checked);
                if (nodeInfoArray[i].checkBox.Checked == false)
                    continue;
                
                for (j = 0; j < 10; ++j)
                {
                    if (nodeInfoArray[i].portChecked[j] == 0x01)
                        return true;
                }
            }
            return false;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            
            //timerSend.Stop();
            if (!checkOk())
            {
                //MessageBox.Show("没有选中开关设备或没有选中端口");
                Notify notify = new Notify("没有选中开关设备或没有选中端口");
                notify.ShowDialog();
                return;
            }
            //if (paused == true)
            if(ringFlag == false)
            {
                if (comboBox10.Text.Trim().Length == 0)
                {
                    saveTime = 5  * 60;
                    //MessageBox.Show("数据保存时间输入错误，自动默认为5分钟");
                    Notify notify = new Notify("数据保存时间输入错误，自动默认为5分钟");
                    notify.ShowDialog();
                }
                else
                    saveTime = Int32.Parse(comboBox10.Text)  * 60;


                Int32 timCycle = 0,timWork = 0;
                try
                {
                    timCycle = Int32.Parse(comboBox3.Text) * 1000 + Int32.Parse(comboBox2.Text) * 1000 * 60 + Int32.Parse(comboBox1.Text) * 3600 * 1000;
                    timWork = Int32.Parse(comboBox9.Text) * 1000 + Int32.Parse(comboBox8.Text) * 1000 * 60 + Int32.Parse(comboBox7.Text) * 3600 * 1000;
                }
                catch (Exception)
                {
                    Notify notify = new Notify("没有输入正确的时间");
                    notify.ShowDialog();
                    //MessageBox.Show("没有输入正确的时间");
                    return;
                }
                if(timCycle < 21000)
                {
                    Notify notify = new Notify("端口循环时间不得低于20秒");
                    notify.ShowDialog();
                 //   MessageBox.Show("端口循环时间不得低于20秒");
                    return;
                }
                if (timWork != 0)
                {
                    if (timWork < 41000)
                    {
                        Notify notify = new Notify("工作时间不得低于40秒");
                        notify.ShowDialog();
                        //MessageBox.Show("工作时间不得低于40秒");
                        return;
                    }
                    timer1.Interval = timWork;
                    timer1.Start();
                    workFlag = true;
                }
                timCycle /= 1000;
                
                timeHours = timCycle / 3600; timeHoursInit = timeHours;
                timeMinutes = (timCycle % 3600) / 60; timeMinutesInit = timeMinutes;
                timeSeconds = timCycle % 60; timeSecondsInit = timeSeconds;

                //首先关闭其他节点
                sendClose();
                toolStripStatusLabel1.Text = "当前正在准备循环发送";
                                //启动
                Resume();
                ringFlag = true;
               // button2.Text = "中断循环";
                groupBox6.Enabled = false;
                groupBox9.Enabled = false;
                groupBox12.Enabled = false;
                groupBox8.Enabled = false;
                groupBox10.Enabled = false;
                oneFlag = false;
                label3.Text = "剩余时间:";
            }
            else
            {
                toolStripStatusLabel1.Text = "循环发送停止";
                timer1.Stop();
                Pause();
                sendClose();

                clearTimeCount();

                //button2.Text = "启动循环";
                workFlag = false;
                ringFlag = false;
                groupBox6.Enabled = true;
                groupBox9.Enabled = true;
                groupBox12.Enabled = true;
                groupBox8.Enabled = true;
                groupBox10.Enabled = true;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //挂起循环发送线程
            //Pause();
            //停止工作时间定时器
            //timer1.Stop();
            //timerSend.Stop();
            if (!checkOk())
            {
                //MessageBox.Show("没有选中开关设备或没有选中端口");
                Notify notify = new Notify("没有选中开关设备或没有选中端口");
                notify.ShowDialog();
                return;
            }
            if (cycleFlag == false)
            {
                if (comboBox10.Text.Trim().Length == 0)
                {
                    saveTime = 5  * 60;
                    Notify notify = new Notify("数据保存时间输入错误，自动默认为5分钟");
                    notify.ShowDialog();
                   //MessageBox.Show("数据保存时间输入错误，自动默认为5分钟");
                }
                else
                    saveTime = Int32.Parse(comboBox10.Text)  * 60;

                Int32 timeRound = Int32.Parse(comboBox6.Text) * 1000 + Int32.Parse(comboBox5.Text) * 1000 * 60 + Int32.Parse(comboBox4.Text) * 3600 * 1000;
                try
                {

                    timerSend.Interval = timeRound;
                }
                catch (Exception)
                {
                    Notify notify = new Notify("时间设置有误，请重新输入");
                    notify.ShowDialog();
                    //MessageBox.Show("时间设置有误，请重新输入");
                    return;
                }
                timeRound /= 1000;

                timeHours = timeRound / 3600; timeHoursInit = timeHours;
                timeMinutes = (timeRound % 3600) / 60; timeMinutesInit = timeMinutes;
                timeSeconds = timeRound % 60; timeSecondsInit = timeSeconds;
                toolStripStatusLabel1.Text = "启动周期发送命令,正在准备中";
                cycleFlag = true;
                isFirstCycleFlag = true;
                groupBox5.Enabled = false;
                groupBox13.Enabled = false;
                groupBox9.Enabled = false;
                groupBox10.Enabled = false;
                oneFlag = false;
                label3.Text = "剩余时间:";
                cycleSend();
                timerSend.Start();
                //button3.Text = "终止周期";

            }
            else
            {
                toolStripStatusLabel1.Text = "结束周期发送命令";
                clearTimeCount();
                groupBox5.Enabled = true;
                groupBox13.Enabled = true;
                groupBox9.Enabled = true;
                groupBox10.Enabled = true;
                timerSend.Stop();
                sendClose();
                cycleFlag = false;
                isFirstCycleFlag = true;
               // button3.Text = "周期发送";
                this.pictureBox1.Image = global::AccleZigBee.Properties.Resources.off2;
            }
        }
        public void setPortAngle(int i, string angle)
        {
            nodeInfoArray[i].angle = angle;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[0] != 0x02)
            {
                if (checkBox3.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[0] = 0x01;//该端口的状态为checked
                else
                nodeInfoArray[openingRouterIndex].portChecked[0] = 0x00;//该端口为被选中
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[1] != 0x02)
            {
                if (checkBox4.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[1] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[1] = 0x00;//该端口为被选中
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[2] != 0x02)
            {
                if (checkBox5.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[2] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[2] = 0x00;//该端口为被选中
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[3] != 0x02)
            {
                if (checkBox6.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[3] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[3] = 0x00;//该端口为被选中
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[4] != 0x02)
            {
                if (checkBox7.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[4] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[4] = 0x00;//该端口为被选中
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[5] != 0x02)
            {
                if (checkBox8.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[5] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[5] = 0x00;//该端口为被选中
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[6] != 0x02)
            {
                if (checkBox9.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[6] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[6] = 0x00;//该端口为被选中
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[7] != 0x02)
            {
                if (checkBox10.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[7] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[7] = 0x00;//该端口为被选中
            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[8] != 0x02)
            {
                if (checkBox11.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[8] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[8] = 0x00;//该端口为被选中
            }
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            //端口可用
            if (nodeInfoArray[openingRouterIndex].portChecked[9] != 0x02)
            {
                if (checkBox12.Checked)//端口被选中
                    nodeInfoArray[openingRouterIndex].portChecked[9] = 0x01;//该端口的状态为checked
                else
                    nodeInfoArray[openingRouterIndex].portChecked[9] = 0x00;//该端口为被选中
            }
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Construct construct = new Construct();
            construct.ShowDialog();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Construct construct = new Construct();
            construct.ShowDialog();
        }
        private void sendClose()
        {
            int n = nodeInfoArray.Count;
            for (int i = 0; i < n; ++i)
            {
                if (nodeInfoArray[i].NodeName == 0x02) continue;
                if (nodeInfoArray[i].checkBox.Checked == false) continue;
                sendMsg(i, 0x0000);
            }
 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //时间到
            //如果在工作，则
            if (workFlag)
            {
                int timWork = Int32.Parse(comboBox9.Text) * 1000 + Int32.Parse(comboBox8.Text) * 1000 * 60 + Int32.Parse(comboBox7.Text) * 3600 * 1000;
                timWork /= 1000;

                timeHours = timWork / 3600;
                timeMinutes = (timWork % 3600) / 60;
                timeSeconds = timWork % 60;
                workFlag = false;
                toolStripStatusLabel1.Text = "工作时间已到，停止工作并等待下一个周期";
                Pause();
                sendClose();
            }
            else
            {
                clearTimeCount();
                workFlag = true;
                toolStripStatusLabel1.Text = "休眠期已到，继续工作并在一个周期后停止";
                sendClose();
                Resume();
            }
        }

        private void timerOut_Tick(object sender, EventArgs e)
        {
            count += 1;
            if (count >= 4294967290)
            {
                count = 0;
            }
            refreshTime();
        }
        private void refreshTime()
        {
            if ((ringFlag == false) && (cycleFlag == false))
            {
                if (oneFlag == true)
                {
                    ++timeSeconds;
                    if (timeSeconds >= 60)
                    {
                        ++timeMinutes;
                        timeSeconds = 0;
                    }
                    if (timeMinutes >= 60)
                    {
                        ++timeHours;
                        timeMinutes = 0;
                    }
                    updateTimeLabel(timeHours, timeMinutes, timeSeconds);
                }
                return;
            }
            if ((timeSeconds--) <= 0)//timesecond为0
            {
                if (timeMinutes != 0)
                {
                    timeMinutes--;
                    timeSeconds = 59;
                }
                else
                {
                    if (timeHours != 0)
                    {
                        timeHours--;
                        timeMinutes = 59;
                        timeSeconds = 59;
                    }
                    else
                    {
                        timeHours = 0;
                        timeMinutes = 0;
                        timeSeconds = 0;
                    }
                }
            }
            updateTimeLabel(timeHours, timeMinutes, timeSeconds);
        }
        private void updateTimeLabel(int h, int m, int s)
        {
            this.hourLabel.Text = h.ToString();
            this.minuteLabel.Text = m.ToString();
            this.secondsLabel.Text = s.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //sendClose();
            Pause();
            timerSend.Stop();
            timer1.Stop();
            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
            groupBox9.Enabled = true;
            groupBox12.Enabled = true;
            groupBox8.Enabled = true;
            groupBox10.Enabled = true;     
            groupBox13.Enabled = true;
           // groupBox9.Enabled = true;
            //comboBox10.Enabled = true;
            checkBox14_CheckedChanged(null, null);
            clearTimeCount();
            workFlag = false;
            ringFlag = false;
            cycleFlag = false;
            SendPacket.Clear();
        }

        private void groupBox10_MouseHover(object sender, EventArgs e)
        {
            GroupBox pb = (GroupBox)sender;
            tips.SetToolTip(pb, "设置加速度上线多长时间后开始记录数据");
        }

        public void updateAngle(string name)
        {
            string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + name + @".txt";
            if (!File.Exists(tpath))
            {
                Notify notify = new Notify("未找到该开关节点的端口映射表，不能配置角度文件");
                notify.ShowDialog();
              //  MessageBox.Show("未找到该开关节点的端口映射表，不能配置角度文件");

                return;
            }
            using (StreamReader sr = new StreamReader(tpath))
            {
                string line;
                string angleString;
                if ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split(new Char[] { ';' });
                    int num = split.Length;
                    //newNode.portNum = split.Length;
                    for (int j = 0; j < num; ++j)
                    {
                        line = split[j].Trim();
                        if (line.Length != 0)
                        {
                            //line为名字
                           
                           angleString = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                           if (!File.Exists(angleString))
                           {
                               Notify notify = new Notify("未找到某角度文件");
                               notify.ShowDialog();
                                // MessageBox.Show("未找到某角度文件");
                                return;
                            }
                            using (StreamReader sr1 = new StreamReader(angleString))
                            {
                                angleString = sr1.ReadLine().Trim();
                               // Console.WriteLine("mainhome---1612-----{0}",line);
                                if(nodeNameIndexHash.ContainsKey(line))
                                {
                                    if (angleString.Length == 0)
                                        nodeInfoArray[nodeNameIndexHash[line]].angle = "NULL";
                                    else
                                    {
                                        nodeInfoArray[nodeNameIndexHash[line]].angle = angleString;
                                    }
                                }
                                sr1.Close();
                            }//using

                           // Console.WriteLine("正在更新{0}的角度数据文件，角度为{1}", line, angleString);
                        }
                    }
                    sr.Close();
                }
                else
                {
                    Notify notify = new Notify("文件存在但是其中并没有数据，请检查配置文件" + name);
                    notify.ShowDialog();
                   // MessageBox.Show("文件存在但是其中并没有数据，请检查配置文件" + name);
                }
            }//using
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            saveTime = Int32.Parse(comboBox10.Text) * 60;
        }



        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                isSameAngle = true;
                checkBox2.Checked = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                isSameAngle = false;
                checkBox1.Checked = false;
                Config config = new Config(this);
                config.ShowDialog();
            }
        }

        private void radioButtonCheckedChanged(object sender, EventArgs e)
        {
            RadioButton tBtn = (RadioButton)(sender);
            if (tBtn.Checked)
            {
                checkBox1.Checked = true;
                sameAngle = int.Parse(tBtn.Text);
                if (isSameAngle)
                { 
                    int n = nodeInfoArray.Count;
                    for(int i= 0;i <n;++i)
                    {
                        if (nodeInfoArray[i].NodeName == 0x02)
                        {
                            clearPacketCnt(nodeInfoArray[i].name);
                        }
                    }
                }
             //   Console.WriteLine("{0}", sameAngle);
            }
        }
    }
}
