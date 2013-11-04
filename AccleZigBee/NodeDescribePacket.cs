using System;
using System.Windows.Forms;
namespace AccleZigBee
{
    public class NodeDescribePacket
    {
 #region 描述符
        //节点类型
        public byte NodeName;//&&
        //网络地址
        public ushort NetAddr;//&&
        //父亲地址
        public ushort ParentAddr;//&&
        //硬件地址
        public string Mac;//&&
        //记录节点的名字
        public string name;//&&
        //传感器数目
        public byte SensorNum;//&&
        //记录每个网关节点的端口上设备的MAC地址
        public string[] port;//&&
       // public int portNum;
        public UInt16 onFlag;
        //public ArrayList ClustAndCidList;  //动态构造传感器编号和类型
#if false
        //因为只有一个加速度传感器，所以这里没有使用ArrayList
        public ClustAndCid clustAndCidList;
#endif
        public UInt32 count;
        public UInt32 packetCount;
 #endregion


#region 日志相关
        //当前节点存放数据的路径
        public string path;
        public string myDataPath;
#endregion

#region 节点显示相关
        //当前节点在拓扑结构中的坐标
        public myPoint point;
        //当前节点虚拟的坐标
        public myPoint virtualPoint;
        //周围空位是否已经使用
        public bool[] used;
        //虚拟节点周围是否已经使用
        public bool[] virtualUsed;
        public int[] indexNo;
        //在控制面板中的位置（对于网关为toolstripbutton右边的checkbox位置，对于终端则为toolstripbutton右边的datalabel位置）
        public int cx;
        public int cy;  
#endregion
#region 数据相关
        //是否满30帧数据的标志
        public bool flag;
        public int head, tail;  //
        //用于均值滤波的数组
        public double[] data;
        //用于存放标定值的数组
        public double[] config;
        //求当前30组数组的和，不满30组也允许
        public double sum;
        public string angle;
        public double avg;
        public byte dataSaveCount;
        //记录当前节点数据在excel中的行
        public int line;
        //用于标记是否开始记录节点数据，意味着需要在excel中增加新的行
#endregion

 #region 控件
        //网络拓扑图标
        public System.Windows.Forms.PictureBox pictureBox;
        public System.Windows.Forms.ToolStripButton nodeBtn;
        public Label dataLabel;
        public LinkLabel linkLabel;
        public CheckBox checkBox;
#endregion

#region 节点状态
        //节点的打开状态
        public byte status;
        //记录网关节点的某个端口是否可用，是否选中（0x02不可用 0x01被选中 0x00未被选中）
        public byte [] portChecked;
#endregion
 
        //构造函数
        public NodeDescribePacket()
        {
            onFlag = 0x0000;
          //  angle = 0;
            nodeBtn = new ToolStripButton();
            checkBox = new CheckBox();
            nodeBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            nodeBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            port = new string[10];
            portChecked = new byte[10];
            dataLabel = new Label();
            linkLabel = new LinkLabel();
            dataLabel.AutoSize = true;
            linkLabel.Size = new System.Drawing.Size(77, 22);
            dataLabel.Size = new System.Drawing.Size(50, 22);
            linkLabel.Text = "数据";
            //nodeBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            //初始化循环队列
            head = tail = 0;
            //初始化节点图标
            pictureBox = new System.Windows.Forms.PictureBox();
#if false
            clustAndCidList = new ClustAndCid();
#endif
            point = new myPoint();
            virtualPoint = new myPoint();
            used = new bool[8];  //每个节点周围位置可用的标志
            virtualUsed = new bool[8];
            indexNo = new int[8];
            config = new double[5];
            flag = false;
            data = new double[27];
            sum = 0;
            avg = 0;
            dataSaveCount = 1;
            status = 0x00;
            packetCount = 0;
          //  writeFaile = false;
            //初始化都不能使用
            for (int i = 0; i < 10; ++i)
                portChecked[i] = 0x02;
                for (int i = 0; i < 8; i++)
                {
                    used[i] = false;
                    virtualUsed[i] = false;
                    indexNo[i] = 0;
                }
            //    isSave = false;
        }
        public void inptData(double myData)
        {
            if (flag) //已经收到了26组数据
            {
                sum += myData;
                sum -= data[head];
                head = (head + 1) % 26;
                tail = (tail + 1) % 26;
                data[tail] = myData;
                return;
            }
            if ((tail + 1) % 26 == head)//至此已经找到了完整的26组数据,循环队列的标志由tail指向下一个位置转化为指向最后一个位置
            {
                //head ==0,tail=25;
                flag = true;
                data[tail] = myData;
                sum += myData;
                 //   Console.WriteLine("马上满，head={0},tail={1}", head, tail);
                // tail = (tail + 1) % 26;

            }
            else
            {
                data[tail] = myData;
                sum += myData;
               //  Console.WriteLine("距离满还早，head={0},tail={1}", head, tail);
                tail = (tail + 1) % 26;
            }
        }
    }
    public class myPoint
    {
        public int x;
        public int y;
        public double angle;//角度
        public myPoint(int xx, int yy, double iangle)
        {
            x = xx;
            y = yy;
            angle = iangle;
        }
        public myPoint()
        {
            x = 0;
            y = 0;
            angle = 0;
        }
    }
    public class ClustAndCid
    {
        public byte sensorType;             //传感器类型
        public int Cid;                    //编号
    }

    public class CtlPacket
    {
        //发送串口数据的缓冲
        private byte[] buffer ;
        private System.IO.Ports.SerialPort serial;
        private byte cnt;
        public CtlPacket(System.IO.Ports.SerialPort tPort, ushort addr, UInt16 ctlWord, byte tcnt)
        {
            serial = tPort;
            buffer = new byte[9];
            buffer[0] = 0xfe;
            buffer[1] = 0x09;
            buffer[2] = 0x05;
            buffer[3] = (byte)(addr >> 8); //高8位
            buffer[4] = (byte)(addr & 0x00FF);//低8位
            buffer[5] = (byte)(ctlWord >> 8); ;//高8位
            buffer[6] = (byte)(ctlWord & 0x00FF);//低8位
            buffer[7] = tcnt;
            buffer[8] = 0xfb;
            cnt = tcnt;
        }
        public byte doTask()
        {
           // for (int i = 0; i < 9; i++)
            //    Console.WriteLine("{0}--{1}",i,buffer[i]);
            serial.Write(buffer, 0, 9);
            /*
            string path = Directory.GetCurrentDirectory();
            path = path + @"\sendLog.txt";

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(DateTime.Now.ToString() + " 发送编号为"+cnt.ToString());
                sw.Close();
            }*/
            return cnt;
        }
    }
}
