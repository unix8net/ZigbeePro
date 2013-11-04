using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace AccleZigBee
{
    public partial class Config : Form
    {
        private string file;
        //是否满30帧数据的标志
        public bool flag;
        public int head, tail;  //
        //用于均值滤波的数组
        public double[] data;
        //用于存放标定值的数组
        public double[] config;
        //求当前30组数组的和，不满30组也允许
        public double sum;
        public string newName;
        public Config(MainHome tHome)
        {
            InitializeComponent();
            mainHome = tHome;
            checkBox1.Checked = mainHome.getFilter();
            checkBox2.Checked = mainHome.getModify();
            checkBox4.Checked = checkBox4.Checked = mainHome.getLog();
            flag = false;
            data = new double[27];
            sum = 0;
            head = tail = 0;
        }



        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                mainHome.setFilter(true);
            }
            else
            {
                mainHome.setFilter(false);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                mainHome.setModify(true);
            }
            else
            {
                mainHome.setModify(false);
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                mainHome.setLog(true);
            }
            else
            {
                mainHome.setLog(false);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\config.txt";
            //path = path + ((NodeDescribePacket)NodeDesList[indexMenu]).Mac;
            //Console.WriteLine("{0}",path);
            System.Diagnostics.Process.Start("notepad.exe", path);
        }

        //private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    string path = System.AppDomain.CurrentDomain.BaseDirectory + @"Data\";
        //    //path = path + ((NodeDescribePacket)NodeDesList[indexMenu]).Mac;
        //    Console.WriteLine("{0}", System.AppDomain.CurrentDomain.BaseDirectory);
        //    System.Diagnostics.Process.Start("explorer.exe", path);
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            ieee1.Text = ieee1.Text.ToUpper();
            if (ieee1.Text.Length != 16)
            {
                MessageBox.Show("请输入合法的IEEE地址");
                return;
            }
            name1.Text = name1.Text.ToUpper();
            if (name1.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名字");
                return;
            }
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\IEEE.txt";
            delIEEE(ieee1.Text);
            addIEEE(ieee1.Text, name1.Text);
            MessageBox.Show("添加完成！");


        }
        private void addIEEE(string ieee, string name)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\IEEE.txt";
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(ieee);
                sw.WriteLine(name);
                sw.Close();
            }
        }

        private void delIEEE(string ieee)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\IEEE.txt";
            //读取所有行
            List<string> lines = new List<string>(File.ReadAllLines(path));
            for (int i = 0; i < lines.Count; ++i)
            {
                //查找是否已经存在        
                try
                {
                    if ((lines[i] == ieee) &&(lines[i+1].Trim().Length !=0))
                    {
                        lines.RemoveAt(i);
                        lines.RemoveAt(i);
                        break;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("IEEE与通用名文件存在错误，请检查，该次操作失效");
                    return;
                }
            }
            File.WriteAllLines(path, lines.ToArray());
        }

        //删除其中的一个样例
        private void button2_Click(object sender, EventArgs e)
        {
            if (ieee1.Text.Length != 16)
            {
                MessageBox.Show("请输入合法的IEEE地址");
                return;
            }
            delIEEE(ieee1.Text);
            MessageBox.Show("删除完成！");
        }

        //增加端口映射
        private void button4_Click(object sender, EventArgs e)
        {
            //先清空
            if (textBox3.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名");
                return;
            }
            delPort();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + textBox3.Text + @".txt";
            if (!File.Exists(path))
                File.CreateText(path).Close();
            using (StreamWriter sw = File.AppendText(path))
            {
                path = "";
                textBox4.Text = textBox4.Text.ToUpper();
                if (checkText(textBox4.Text))
                    path = textBox4.Text + ";";
                else
                    path = ";";
                textBox5.Text = textBox5.Text.ToUpper();
                if (checkText(textBox5.Text))
                    path += (textBox5.Text + ";");
                else
                    path += ";";
                textBox6.Text = textBox6.Text.ToUpper();
                if (checkText(textBox6.Text))
                    path += (textBox6.Text + ";");
                else
                    path += ";";
                textBox7.Text = textBox7.Text.ToUpper();
                if (checkText(textBox7.Text))
                    path += (textBox7.Text + ";");
                else
                    path += ";";
                textBox8.Text = textBox8.Text.ToUpper();
                if (checkText(textBox8.Text))
                    path += (textBox8.Text + ";");
                else
                    path += ";";
                textBox9.Text = textBox9.Text.ToUpper();
                if (checkText(textBox9.Text))
                    path += (textBox9.Text + ";");
                else
                    path += ";";

                textBox10.Text = textBox10.Text.ToUpper();
                if (checkText(textBox10.Text))
                    path += (textBox10.Text + ";");
                else
                    path += ";";

                textBox11.Text = textBox11.Text.ToUpper();
                if (checkText(textBox11.Text))
                    path += (textBox11.Text + ";");
                else
                    path += ";";

                textBox12.Text = textBox12.Text.ToUpper();
                if (checkText(textBox12.Text))
                    path += (textBox12.Text + ";");
                else
                    path += ";";

                textBox13.Text = textBox13.Text.ToUpper();
               // Console.WriteLine("{0}", textBox13.Text);
                if (checkText(textBox13.Text))
                {
                    path += (textBox13.Text + " ");// Console.WriteLine("111");
                }
                else
                {
                    path += " "; //Console.WriteLine("222");
                }
                sw.WriteLine(path);
                //Console.WriteLine("{0}-{1}",path,textBox13.Text);
                sw.Close();
                MessageBox.Show("增加完成！");
            }

        }
        private bool checkText(string name)
        {
            if ((name.Trim().Length == 4) || (name.Trim().Length == 0))
            {
                return true;
            }
            //Console.WriteLine("{0}...{1}",name, name.Trim);
            return false;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名");
                return;
            }
            delPort();
             MessageBox.Show("删除完成！");
        }
        private void delPort()
        {
            if (textBox3.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名字");
                return;
            }
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + textBox3.Text + @".txt";
            FileStream stream2 = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            stream2.Seek(0, SeekOrigin.Begin);
            stream2.SetLength(0); //清空txt文件
            stream2.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 4)
            {
                MessageBox.Show("请输入正确的设备名");
                return;
            }
            textBox1.Text = textBox1.Text.ToUpper();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"Data\" + textBox1.Text;
           // Console.WriteLine("{0}",path);
            if (!Directory.Exists(path))
            {
                MessageBox.Show("没有该名字对应的数据目录，可能是输入出错或者输入的并非加速度节点");
                return;
            }
           // Console.WriteLine("当前的选择路径为{0}",path);
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = path;
            //fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "所有文件(*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                //源文件
                file = fileDialog.FileName; 
                //获取mac
                string mac = mainHome.readMacByName(textBox1.Text);
                if (mac == null)
                {
                    MessageBox.Show("V应该大写输入");
                    return;
                }
                string line;
                double a1=0;
                double a2=0;
                double a3=0;
                double a4=0;
                double a5=0;
                string tpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\config.txt";
                using (StreamReader sr = new StreamReader(tpath))
                {
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line ==mac)
                        {
                            line = sr.ReadLine();

                            string[] split = line.Split(new Char[] { ';' }); 
                           /* Console.WriteLine("{0}", split[0]); 
                            Console.WriteLine("{0}", split[1]);
                            Console.WriteLine("{0}", split[2]);
                            Console.WriteLine("{0}", split[3]);
                            Console.WriteLine("{0}", split[4]);*/
                            a1 = System.Convert.ToDouble(split[0]);
                            a2 = System.Convert.ToDouble(split[1]);
                            a3 = System.Convert.ToDouble(split[2]);
                            a4 = System.Convert.ToDouble(split[3]);
                            a5 = System.Convert.ToDouble(split[4]);
                            sr.Close();
                            break;
                        }
                    }//while
                    
                }//using

                //Console.WriteLine("{0}/{1}/{2}/{3}/{4}",a1,a2,a3,a4,a5);

                bool flag7 = checkBox7.Checked;
                bool flag8 = checkBox8.Checked;
                double data3;
                double tmpdata3;
                double retdata3;
                newName = file + "Convert";
                flag = false;
                sum = 0;
                head = tail = 0;
                if (!File.Exists(newName))
                {
                    File.CreateText(newName).Close();
                }
                else
                {
                    FileStream stream2 = File.Open(newName, FileMode.OpenOrCreate, FileAccess.Write);
                    stream2.Seek(0, SeekOrigin.Begin);
                    stream2.SetLength(0); //清空txt文件
                    stream2.Close();
                }
                using (StreamReader sr = new StreamReader(file))
                {
                    using (StreamWriter sw = File.AppendText(newName))
                    {
                        line = sr.ReadLine();
                        while ((line = sr.ReadLine()) != null)
                        {
                            data3 = Convert.ToDouble(line);
                            inptData(data3);
                            //滤波选项
                            if (flag7)
                            {
                                if (flag == false)
                                {
                                    data3 = sum / (tail - head);
                                }
                                else
                                {
                                    data3 = sum / 26;
                                }
                            }
                            //标定
                            if (flag8)
                            {
                                tmpdata3 = data3;
                                retdata3 = a5;
                                retdata3 += a4 * tmpdata3; tmpdata3 *= data3;
                                retdata3 += a3 * tmpdata3; tmpdata3 *= data3;
                                retdata3 += a2 * tmpdata3; tmpdata3 *= data3;
                                retdata3 += a1 * tmpdata3;
                                data3 = retdata3;
                               
                            } 
                            sw.WriteLine(data3.ToString());
                        }
                    }

                    // MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
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
        private void updateAngle(string path, TextBox t)
        {
            //Console.WriteLine("{0}",path);
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {  
                    path = sr.ReadLine();
                    if (path != null)
                    {
                        t.Text = path.Trim();
                    }
                    sr.Close();
                }//using
            }
        }
        private void button10_Click(object sender, EventArgs e)
        {
            //先清空
            if (textBox3.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名");
                return;
            }
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + textBox3.Text + @".txt";
            if (!File.Exists(path))
            {
                MessageBox.Show("未找到该名字对应的信息，请手动逐个添加");
                return;
            }
            textBox22.Text = textBox3.Text = textBox3.Text.ToUpper();
            this.button10.Enabled = false;
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                string basePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\";
                if ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split(new Char[] { ';' });
                    if (split[0].Length != 0)
                    {
                        textBox4.Text = split[0];
                        updateAngle(basePath  + split[0],textBox21);
                    }
                    if (split[1].Length != 0)
                    {
                        textBox5.Text = split[1];
                        updateAngle(basePath + split[1], textBox2);
                    }
                    if (split[2].Length != 0)
                    {
                        textBox6.Text = split[2];
                        updateAngle(basePath + split[2], textBox19);
                    }
                    if (split[3].Length != 0)
                    {
                        textBox7.Text = split[3];
                        updateAngle(basePath + split[3], textBox20);
                    }
                    if (split[4].Length != 0)
                    {
                        textBox8.Text = split[4];
                        updateAngle(basePath + split[4], textBox23);
                    }
                    if (split[5].Length != 0)
                    {
                        textBox9.Text = split[5];
                        updateAngle(basePath + split[5], textBox28);
                    }
                    if (split[6].Length != 0)
                    {
                        textBox10.Text = split[6];
                        updateAngle(basePath + split[6], textBox27);
                    }
                    if (split[7].Length != 0)
                    {
                        textBox11.Text = split[7];
                        updateAngle(basePath + split[7], textBox26);
                    }
                    if (split[8].Length != 0)
                    {
                        textBox12.Text = split[8];
                        updateAngle(basePath + split[8], textBox25);
                    }
                    if (split[9].Length != 0)
                    {
                        textBox13.Text = split[9];
                        updateAngle(basePath + split[9], textBox24);
                    }
                    sr.Close();
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!File.Exists(newName))
            {
                 MessageBox.Show("不存在转换后文件");
                 return;
            }
            System.Diagnostics.Process.Start("notepad.exe", newName);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (mainHome.isSameAngle)
            {
                MessageBox.Show("当前设置为所有加速度为相同角度，想分别设置角度，请在主界面中勾选<分别设置>后重试");
                return;
            }
            if (textBox22.Text.Length != 4)
            {
                MessageBox.Show("请输入合法的通用名");
                return;
            }
            textBox22.Text = textBox22.Text.ToUpper();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + textBox22.Text + @".txt";
           if (!File.Exists(path))
           {
               MessageBox.Show("未找到该名字对应的信息，请先添加端口映射");
               return;
           }
           using (StreamReader sr = new StreamReader(path))
           {
               string line;
               if ((line = sr.ReadLine()) != null)
               {
                   //读取端口映射
                   string[] split = line.Split(new Char[] { ';' });
                   string creatName;

                   //line为端口映射的节点名字
                   line = split[0].Trim();
                   if(line.Length != 0)
                   {

                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       //将新角度写入到文件中
                       if(makeFile(creatName,textBox21.Text))
                            mainHome.clearPacketCnt(line);
                   }
                   line = split[1].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox2.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[2].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox19.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[3].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox20.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[4].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox23.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[5].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox28.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[6].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox27.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[7].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox26.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[8].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if(makeFile(creatName, textBox25.Text))
                       mainHome.clearPacketCnt(line);
                   }
                   line = split[9].Trim();
                   if (line.Length != 0)
                   {
                       creatName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Angle\" + line;
                       if (makeFile(creatName, textBox24.Text))
                       mainHome.clearPacketCnt(line);
                   }

                   mainHome.updateAngle(textBox22.Text);
                   MessageBox.Show("更新完成");

                  
               }
               else
               {
                   string about = "没有端口映射文件，请先添加端口映射表";
                   MessageBox.Show(about);
               } 
               sr.Close();
           }//using

        }

        public bool makeFile(string name, string data)
        {
            if (!File.Exists(name))
                File.CreateText(name).Close();
            else
            {
                string angle;
                //首先检查文件中的角度是否是和新角度相同
                using (StreamReader sr = new StreamReader(name))
                {
                    angle = sr.ReadLine();
                    if (angle == data)
                    {
                        sr.Close();
                        return false;
                    }
                    sr.Close();
                }//using
                FileStream stream2 = File.Open(name, FileMode.OpenOrCreate, FileAccess.Write);
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.SetLength(0); //清空txt文件
                stream2.Close();
            }
            using (StreamWriter sw = File.AppendText(name))
            {
                sw.WriteLine(data);
                sw.Close();
            }
            return true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            FileView myFileView = new FileView();
            myFileView.ShowDialog();
           // myFileView.update();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //FileCopy fileCopy = new FileCopy(System.AppDomain.CurrentDomain.BaseDirectory + @"\Data\data.xls", System.AppDomain.CurrentDomain.BaseDirectory);
            //fileCopy.ShowDialog();
            File.Copy(System.AppDomain.CurrentDomain.BaseDirectory + @"\Data\data.xls", System.AppDomain.CurrentDomain.BaseDirectory + @"\Data\data_copy.xls", true);
            MessageBox.Show("复制完成");

        }

        private void button13_Click(object sender, EventArgs e)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"Data\";
            //path = path + ((NodeDescribePacket)NodeDesList[indexMenu]).Mac;
            //Console.WriteLine("{0}", System.AppDomain.CurrentDomain.BaseDirectory);
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            textBox21.Text = ""; textBox2.Text = ""; textBox19.Text = ""; textBox20.Text = ""; textBox23.Text = "";
            textBox28.Text = ""; textBox27.Text = ""; textBox26.Text = ""; textBox25.Text = ""; textBox24.Text = "";
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("仅仅查看部分数据，查看更多数据请到Data目录下","数据保护提醒",MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                try
                {
                    System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + @"Data\data1.xls");
                }
                catch (Exception)
                {
                    MessageBox.Show("你的电脑未安装Excle");
                }
            }
            else
            { }
        }
    }
}
