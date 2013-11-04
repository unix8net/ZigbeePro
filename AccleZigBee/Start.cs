using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
namespace AccleZigBee
{
    public partial class Start : Form
    {
        //private MainHome mainHome;
        private Thread setUpThread;
        private Origination or;
        private bool pOk;
        private bool isOpen;
        private bool isP;
        public Start(System.IO.Ports.SerialPort tcomm)
        {
            InitializeComponent();
           
            comm = tcomm;
            pOk = false;
            isOpen = false;
            isP = false;
            string[] ports = SerialPort.GetPortNames();  //自动获取可用的串口名
            comboPortName.Items.AddRange(ports);        //增加到界面
            comboPortName.SelectedIndex = comboPortName.Items.Count > 0 ? 0 : -1;
            comboBaudrate.SelectedIndex = comboBaudrate.Items.IndexOf("115200");
            comboBoxParity.SelectedIndex = comboBoxParity.Items.IndexOf("None");
            comboBoxDataBit.SelectedIndex = comboBoxDataBit.Items.IndexOf("8");
            comboBoxStopBit.SelectedIndex = 0;// comboBoxDataBit.Items.IndexOf("None")
            this.TopMost = true;
            
            setUpThread = new Thread(setUp);
            setUpThread.Start();
            /*
            mainHome = new MainHome(comm);
            or = new Origination(mainHome);
            pOk = true;*/
        }
        private void setUp()
        { 
            mainHome = new MainHome(comm);
            or = new Origination(mainHome);
            pOk = true;
            isP = true;
        }
        private void ok_Click(object sender, EventArgs e)
        {
            if (!isP)
            {
                Notify notify = new Notify("资源正在准备，请稍后...");
                notify.Show();
                return;
            }
            this.timer1.Stop();
            if (!isOpen)
            {
               // if (!comm.IsOpen)
                if(true)
                {
                    //关闭时点击，则设置好端口，波特率后打开
                    comm.PortName = comboPortName.Text;
                    comm.BaudRate = int.Parse(comboBaudrate.Text);
                    comm.DataBits = int.Parse(comboBoxDataBit.Text);
                    switch (comboBoxStopBit.SelectedIndex)
                    {

                        case -1:
                            comm.StopBits = StopBits.One;
                            break;
                        case 0:
                            comm.StopBits = StopBits.One;
                            break;
                        case 1:
                            comm.StopBits = StopBits.Two;
                            break;
                        // comm.StopBits = StopBits.None;
                    }

                    //把字符串转换为eunm枚举类型。
                    comm.Parity = (Parity)Enum.Parse(typeof(Parity), comboBoxParity.SelectedItem.ToString());
                    try
                    {
                        comm.Open(); //打开串口
                        mainHome.startCount();
                    }
                    catch (Exception ex)
                    {
                        //捕获到异常信息，创建一个新的comm对象，之前的不能用了。
                        //comm = new SerialPort();
                        //现实异常信息给客户。
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    isOpen = true;
                }
            }
            //mainHome = new MainHome(comm);
            //Origination or = new Origination(mainHome);
            Thread.Sleep(1);
            if(pOk)
            {
                or.Show();
                this.Hide();
            }
            else
            {
                Notify notify = new Notify("资源正在准备，请稍后...");
                notify.Show();
                return;
            }
            this.timer1.Dispose();
            //串口线程启动
            mainHome.StartSerialPort();

            this.comboBaudrate.Dispose();
            this.comboBoxDataBit.Dispose();
            this.comboBoxStopBit.Dispose();
            this.comboPortName.Dispose();
            this.label1.Dispose();
            this.label2.Dispose();
            this.label3.Dispose();
            this.label4.Dispose();
            this.label5.Dispose();
            //this.label6.Dispose();
            this.label8.Dispose();
            //this.Dispose();
            
        }

        private void Start_FormClosing(object sender, FormClosingEventArgs e)
        {
           // Console.WriteLine("Exit");
            System.Environment.Exit(System.Environment.ExitCode);
            this.Dispose();
            this.Close();
        }
        private void timerCheckComPorts_Tick(object sender, EventArgs e)
        {
            //刷新本机串口，并排序
            //获取当前计算机串口的名称数组
            string[] ports = SerialPort.GetPortNames();
            //判断本机串口是否有变化。
            if (comboPortName.Items.Count != ports.Length)
            {
                //首先清空条目
                comboPortName.Items.Clear();
                //刷新串口数量
                comboPortName.Items.AddRange(ports);
                //设置默认串口
                comboPortName.SelectedIndex = comboPortName.Items.Count > 0 ? 0 : -1;
            }
        }
    }
}
