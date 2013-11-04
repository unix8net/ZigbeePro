using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
//using System.Threading;
namespace AccleZigBee
{
    public partial class NetStruct : Form
    {

        public NetStruct(MainHome tMainHome)
        {
            InitializeComponent();
            mainHome = tMainHome;
            StartPoint = new List<Point>();
            EndPoint = new List<Point>();
            IntPtr i = this.Handle;
            thread = new Thread(new ThreadStart(work));
            thread.Start();
        }


        delegate void ControlCallback(); //无参数

        public void PrepareUpdateNetPicture()
        {
            ControlCallback callBack = new ControlCallback(StartUpdateNetPicture);
            this.BeginInvoke(callBack);
        }

        private List<Point> StartPoint;
        private List<Point> EndPoint;
        private Thread thread;

        void work()
        {
            PrepareUpdateNetPicture();
        }

        /*画图用的变量*/
        private Pen pen = new Pen(Color.Red, 1);//画笔
        private Graphics g;
        private Point sPoint = new Point(), ePoint = new Point();

        #region 更新网络拓扑

        private int level1Num = 8, level2Num = 7;
        private float[] level1Angle = new float[8] { 0, 180, 90, -90, -135, 45, -45, 135 };  //第一层角度分布
        private float level1End = 100;    //第1层终端
        private float level1Router = 150;  //第1层路由器

        private float[] level2Angle = new float[8] { 0, -45, 90, 45, -90, -135, 135, 60 };//第二层角度分布

        private float[] virtualAngle = new float[6] { -45, 90, 45, -90, -135, 135 };//虚拟角度分布
        private float level2End = 50;    //第2层终端
        private float level2Router = 150;  //第2层路由器
        private void StartUpdateNetPicture()
        {
            bool flag = false;
            int dc = 30, dr = 14, de = 16;
            // int w = 624, h = 375;
            int w = (this.panel1.Width == 0) ? 624 : (int)(this.panel1.Width / 2),
                h = (this.panel1.Height == 0) ? 375 : (int)(this.panel1.Height / 2);  //获取拓扑页的中心
            int tmpX, tmpY;
            //总的节点数
            int totalNodeNum = mainHome.nodeInfoArray.Count;
            //this.label1.Text = "[ZigBee加速度网表控制中心] | 当前时间为[" + DateTime.Now.Date.ToShortDateString() + "] | 当前共[" + totalNodeNum.ToString() + "]个节点在线";
            double angle = 0.0;
            //this.panel1.Controls.Clear();//清除        
            StartPoint.Clear();
            EndPoint.Clear();           //清除后为下一次做准备
            int i, j, k, index;
            NodeDescribePacket parentNode = new NodeDescribePacket();       //父亲节点
            NodeDescribePacket virtualParentNode = new NodeDescribePacket(); //虚拟节点，虚拟作为父节点
            NodeDescribePacket childNode = new NodeDescribePacket();        //子节点
            #region 协调器的子节点处理

            for (i = 0; i < totalNodeNum; ++i) //读取节点循环
            {
                childNode = mainHome.nodeInfoArray[i];
                for (j = 0; j < 8; j++)
                {
                    childNode.used[j] = false;              //每个节点之初都没有节点连接
                    childNode.virtualUsed[j] = false;       //每个节点的虚拟位都没有占用
                }
                if (i == 0)
                {
                    if (childNode.NetAddr != 0x0000)
                    {
                        MessageBox.Show("未发现协调器，或者协调器数据格式错误，请断开连接重新启动");
                        return;

                    }
                    //更新协调器节点的位置
                    parentNode = mainHome.nodeInfoArray[0];
                    parentNode.point.x = w;
                    parentNode.point.y = h;
                    AddPictureBox(i, 0x00, w - dc, h - dc, 0);
                }
                else if (childNode.ParentAddr == 0x0000)//先处理协调器的子节点
                {
                    for (k = 0; k < level1Num; k++)
                    {
                        if (!parentNode.used[k]) //找到一个可以使用的点       
                        {
                            angle = Math.PI * level1Angle[k] / 180.0;
                            parentNode.used[k] = true;
                            parentNode.indexNo[k] = i;      //储存
                            break;
                        }
                    }//for(k)
                    if (k >= level1Num) //如果节点的儿子数太多，则进行调整构造
                    {
                        flag = true;
                        break;
                    }//if(k >=R1)

                    if (childNode.NodeName == 0x01)
                    {

                        tmpX = Convert.ToInt32(level1Router * Math.Cos(angle)) + w;
                        tmpY = Convert.ToInt32(level1Router * Math.Sin(angle)) + h;
                        AddPictureBox(i, 0x01, tmpX - dr, tmpY - dr, angle);//图片插入的坐标和画线坐标不同
                        Point point1 = new Point(tmpX, tmpY);
                        StartPoint.Add(point1);

                    }
                    else if (childNode.NodeName == 0x02)
                    {
                        tmpX = Convert.ToInt32(level1End * Math.Cos(angle)) + w;
                        tmpY = Convert.ToInt32(level1End * Math.Sin(angle)) + h;
                        AddPictureBox(i, 0x02, tmpX - de, tmpY - de, angle);//图片插入的坐标和画线坐标不同
                        Point point1 = new Point(tmpX, tmpY);
                        StartPoint.Add(point1);
                    }
                    Point point2 = new Point(w, h);
                    EndPoint.Add(point2);
                }  //else if (childNode.ParentAddr == 0x0000)                 

            } //End for "for (i = readedNodeNum; i <= totalNodeNum; ++i)"



            if (flag)  //拓扑树派生虚拟节点，在虚拟节点上生长叶子
            {
                j = 0; //j用于循环旋转编号
                k = parentNode.indexNo[j];//k表示当前旋转编号对应的zigbee节点,parent在这里为协调器
                virtualParentNode = mainHome.nodeInfoArray[k];
                VirtualOneNode(k, virtualParentNode.NodeName);               //构造虚拟节点
                for (; i < totalNodeNum; ++i)
                {

                    childNode = mainHome.nodeInfoArray[i];  //子节点
                    for (int jj = 0; jj < 8; ++jj)
                    {
                        childNode.used[jj] = false;   //每个节点之初都没有节点连接
                        childNode.virtualUsed[jj] = false;
                    }
                    if (childNode.ParentAddr != 0x0000)//避免无用循环
                        continue;
                    for (index = 0; index < 6; index++)
                    {
                        if (!virtualParentNode.virtualUsed[index])   //查看虚拟节点的虚拟点是否可用
                        {
                            virtualParentNode.virtualUsed[index] = true;
                            angle = Math.PI * virtualAngle[index] / 180.0;
                            break;

                        }

                    }
                    if (index >= 6)  //当前虚拟点已经排满
                    {
                        j++;
                        if (j >= 8)
                        {
                            MessageBox.Show("协调器扩展叶子节点太多，当前节点网络拓扑丢弃，传感信息仍可用");
                            break;
                        }
                        k = parentNode.indexNo[j];                                  //k表示当前旋转编号对应的zigbee节点
                        virtualParentNode = mainHome.nodeInfoArray[k];
                        VirtualOneNode(k, virtualParentNode.NodeName);               //构造虚拟节点
                        i--;                                                        //回退一次，处理当前节点
                        continue;

                    }//if (indexer >= 8) 

                    if (childNode.ParentAddr == 0x0000)//
                    {
                        if (childNode.NodeName == 0x02)//终端
                        {
                            int x = Convert.ToInt32(level2End * Math.Cos(angle));
                            int y = Convert.ToInt32(level2End * Math.Sin(angle));//新坐标
                            angle = virtualParentNode.point.angle;//用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + virtualParentNode.virtualPoint.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + virtualParentNode.virtualPoint.y;//得到旧坐标
                            AddPictureBox(i, 0x02, xx - de, yy - de, virtualParentNode.point.angle + (Math.PI * level2Angle[index] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(virtualParentNode.virtualPoint.x, virtualParentNode.virtualPoint.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }
                        else//路由节点
                        {
                            int x = Convert.ToInt32(level2Router * Math.Cos(angle));
                            int y = Convert.ToInt32(level2Router * Math.Sin(angle));//新坐标
                            angle = virtualParentNode.point.angle;//用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + virtualParentNode.virtualPoint.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + virtualParentNode.virtualPoint.y;//得到旧坐标
                            AddPictureBox(i, 0x01, xx - dr, yy - dr, virtualParentNode.point.angle + (Math.PI * level2Angle[j] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(virtualParentNode.virtualPoint.x, virtualParentNode.virtualPoint.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }
                    }//if (childNode.ParentAddr == 0x0000)

                }//for (; i < totalNodeNum; i++)
            }//if(flag)，处理的是虚拟节点的叶子

            flag = false;
            #endregion


            for (int p = 1; p < totalNodeNum; p++)//父亲
            {
                parentNode = mainHome.nodeInfoArray[p];
                if (parentNode.NodeName != 0x01)//只有路由器才有儿子
                    continue;
                for (i = p + 1; i < totalNodeNum; ++i)//儿子
                {
                    if (p == i)
                        continue;
                    childNode = mainHome.nodeInfoArray[i];
                    if (childNode.ParentAddr == parentNode.NetAddr)//找到子节点
                    {
                        for (j = 0; j < level2Num; j++)
                        {
                            if (!parentNode.used[j])
                            {
                                angle = Math.PI * level2Angle[j] / 180.0;
                                parentNode.used[j] = true;
                                parentNode.indexNo[j] = i;      //储存
                                break;
                            }
                        }
                        if (j >= level2Num)  //节点太多，进行调整构造
                        {
                            // MessageBox.Show("一个zigbee节点的儿子节点超已过7个");
                            flag = true;
                            break;
                        }

                        if (childNode.NodeName == 0x02)//终端
                        {
                            int x = Convert.ToInt32(level2End * Math.Cos(angle));
                            int y = Convert.ToInt32(level2End * Math.Sin(angle));//新坐标
                            angle = parentNode.point.angle;                     //用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + parentNode.point.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + parentNode.point.y;//得到旧坐标
                            AddPictureBox(i, 0x02, xx - de, yy - de, parentNode.point.angle + (Math.PI * level2Angle[j] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(parentNode.point.x, parentNode.point.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }
                        else//路由节点
                        {
                            int x = Convert.ToInt32(level2Router * Math.Cos(angle));
                            int y = Convert.ToInt32(level2Router * Math.Sin(angle));//新坐标
                            angle = parentNode.point.angle;//用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + parentNode.point.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + parentNode.point.y;//得到旧坐标
                            AddPictureBox(i, 0x01, xx - dr, yy - dr, parentNode.point.angle + (Math.PI * level2Angle[j] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(parentNode.point.x, parentNode.point.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }

                    }//if (childNode.ParentAddr == parentNode.NetAddr)
                }//for (i = 1; i< totalNodeNum; ++i)

                // parentNode = (NodeDescribePacket)NodeDesList[p];
                //------------------------------------------
                if (flag)  //拓扑树派生虚拟节点，在虚拟节点上生长叶子
                {
                    j = 0; //j用于循环旋转编号
                    k = parentNode.indexNo[j];//k表示当前旋转编号对应的zigbee节点,parent在这里为路由器
                    // Console.WriteLine("[{0},{1}]-----------",j,k);
                    virtualParentNode = mainHome.nodeInfoArray[k];
                    VirtualOneNode(k, virtualParentNode.NodeName);               //构造虚拟节点
                    for (; i < totalNodeNum; ++i)
                    {
                        if (p == i)
                            continue;
                        childNode = mainHome.nodeInfoArray[i];  //子节点

                        if (childNode.ParentAddr != parentNode.NetAddr)//避免无用循环
                            continue;
                        for (index = 0; index < 6; index++)
                        {
                            if (!virtualParentNode.virtualUsed[index])   //查看虚拟节点的虚拟点是否可用
                            {
                                virtualParentNode.virtualUsed[index] = true;
                                angle = Math.PI * virtualAngle[index] / 180.0;
                                break;

                            }

                        }
                        if (index >= 6)  //当前虚拟点已经排满
                        {
                            j++;
                            if (j >= 7)
                            {
                                MessageBox.Show("路由器扩展叶子节点太多，当前节点网络拓扑丢弃，传感信息仍可用");
                                break;
                            }
                            k = parentNode.indexNo[j];                                  //k表示当前旋转编号对应的zigbee节点
                            virtualParentNode = mainHome.nodeInfoArray[k];
                            VirtualOneNode(k, virtualParentNode.NodeName);               //构造虚拟节点
                            i--;                                                        //回退一次，处理当前节点
                            continue;

                        }//if (indexer >= 8) 


                        if (childNode.NodeName == 0x02)//终端
                        {
                            int x = Convert.ToInt32(level2End * Math.Cos(angle));
                            int y = Convert.ToInt32(level2End * Math.Sin(angle));//新坐标
                            angle = virtualParentNode.point.angle;//用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + virtualParentNode.virtualPoint.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + virtualParentNode.virtualPoint.y;//得到旧坐标
                            AddPictureBox(i, 0x02, xx - de, yy - de, virtualParentNode.point.angle + (Math.PI * level2Angle[index] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(virtualParentNode.virtualPoint.x, virtualParentNode.virtualPoint.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }
                        else//路由节点
                        {
                            int x = Convert.ToInt32(level2Router * Math.Cos(angle));
                            int y = Convert.ToInt32(level2Router * Math.Sin(angle));//新坐标
                            angle = virtualParentNode.point.angle;//用来计算老坐标
                            int xx = Convert.ToInt32(x * Math.Cos(angle) - y * Math.Sin(angle)) + virtualParentNode.virtualPoint.x;
                            int yy = Convert.ToInt32(x * Math.Sin(angle) + y * Math.Cos(angle)) + virtualParentNode.virtualPoint.y;//得到旧坐标
                            AddPictureBox(i, 0x01, xx - dr, yy - dr, virtualParentNode.point.angle + (Math.PI * level2Angle[j] / 180.0));
                            Point point1 = new Point(xx, yy);
                            Point point2 = new Point(virtualParentNode.virtualPoint.x, virtualParentNode.virtualPoint.y);
                            StartPoint.Add(point1);
                            EndPoint.Add(point2);
                            continue;
                        }

                    }//for (; i < totalNodeNum; i++)
                }//if(flag)，处理的是虚拟节点的叶子
                flag = false;
                //------------------------------------------
            }//for (i = 1; i < totalNodeNum; i++)

            this.panel1.Refresh();
            //  Display();
        }
        #endregion

        #region 插入节点图标
        private void AddPictureBox(int no, int type, int x, int y, double r)//相对坐标
        {
            NodeDescribePacket nodePacket = mainHome.nodeInfoArray[no];;
            if (type == 0x01)
            {
                nodePacket.point.x = x + 14;
                nodePacket.point.y = y + 14;
                nodePacket.point.angle = r;
                nodePacket.virtualPoint.x = x + 14 + Convert.ToInt32(60 * Math.Cos(nodePacket.point.angle));
                nodePacket.virtualPoint.y = y + 14 + Convert.ToInt32(60 * Math.Sin(nodePacket.point.angle));
                nodePacket.virtualPoint.angle = r;
            }
            else if (type == 0x02)
            {
                nodePacket.point.x = x + 16;
                nodePacket.point.y = y + 16;
                nodePacket.point.angle = r;
                nodePacket.virtualPoint.x = x + 16 + Convert.ToInt32(60 * Math.Cos(nodePacket.point.angle));
                nodePacket.virtualPoint.y = y + 16 + Convert.ToInt32(60 * Math.Sin(nodePacket.point.angle));
                nodePacket.virtualPoint.angle = r;
            }
            nodePacket.pictureBox.Location = new System.Drawing.Point(x, y);
            if (type == 0x00)
            {
                nodePacket.pictureBox.Name = "0" + ":" + no.ToString() + ":" + mainHome.nodeInfoArray[no].SensorNum.ToString();//这个用于节点的tooltip
                nodePacket.pictureBox.Image = global::AccleZigBee.Properties.Resources.Coor;
                nodePacket.pictureBox.Size = new System.Drawing.Size(60, 60);
            }
            else if (type == 0x01)
            {
                nodePacket.pictureBox.Name = "1" + ":" + no.ToString() + ":" + mainHome.nodeInfoArray[no].SensorNum.ToString();
                nodePacket.pictureBox.Image = global::AccleZigBee.Properties.Resources.Router;
                nodePacket.pictureBox.Size = new System.Drawing.Size(28, 28);
            }
            else
            {
                nodePacket.pictureBox.Name = "2" + ":" + no.ToString() + ":" + mainHome.nodeInfoArray[no].SensorNum.ToString();
                nodePacket.pictureBox.Image = global::AccleZigBee.Properties.Resources.End;
                nodePacket.pictureBox.Size = new System.Drawing.Size(32, 32);
            }

            nodePacket.pictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            nodePacket.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
           // nodePacket.pictureBox.ContextMenuStrip = this.contextMenuStrip1;//右键菜单
           // nodePacket.pictureBox.MouseEnter += new System.EventHandler(this.pictureBoxPhotoEnter);//进入后有tooltips
            this.panel1.Controls.Add(nodePacket.pictureBox);
            //    Console.WriteLine("当前控件数目{0}",this.panel1.Controls.Count);
        }
        #endregion

        private void VirtualOneNode(int no, int type)//当前需要虚拟节点编号+节点类型+父亲节点
        {
            // if (no == 0) return;
            int dr = 14;
            int de = 16;
            int tmpX, tmpY;//临时计算坐标用
            NodeDescribePacket childNode = new NodeDescribePacket();
            childNode = mainHome.nodeInfoArray[no];  //子节点
            if (type == 0x01)
            {
                Point point2 = new Point(childNode.point.x, childNode.point.y);
                EndPoint.Add(point2);
                tmpX = (int)(200 * Math.Cos(childNode.point.angle)) + childNode.point.x;
                tmpY = (int)(200 * Math.Sin(childNode.point.angle)) + childNode.point.y;
                childNode.pictureBox.Location = new System.Drawing.Point(tmpX - dr, tmpY - dr);
                Point point1 = new Point(tmpX, tmpY);
                StartPoint.Add(point1);
                childNode.point.x = tmpX;
                childNode.point.y = tmpY;
            }
            else
            {
                Point point2 = new Point(childNode.point.x, childNode.point.y);
                EndPoint.Add(point2);
                tmpX = (int)(100 * Math.Cos(childNode.point.angle)) + childNode.point.x;
                tmpY = (int)(100 * Math.Sin(childNode.point.angle)) + childNode.point.y;
                childNode.pictureBox.Location = new System.Drawing.Point(tmpX - de, tmpY - de);
                Point point1 = new Point(tmpX, tmpY);
                StartPoint.Add(point1);
                childNode.point.x = tmpX;
                childNode.point.y = tmpY;

            }
        }
        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {
            g = e.Graphics; //this.panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            int n = StartPoint.Count;
            for (int i = 0; i < n; i++)
            {
                //sPoint = (Point)StartPoint[i];
                //ePoint = (Point)EndPoint[i];
                sPoint = StartPoint[i];
                ePoint = EndPoint[i];
                g.DrawLine(pen, sPoint, ePoint);
            }
            //  g.Dispose();
        }

    }
}
