using System;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class Origination : Form
    {
        private MainHome myMainHome;
        public Origination(MainHome mainhome)
        {
            InitializeComponent();
            myMainHome = mainhome;
        }

        private void Origination_Load(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;               //设定ProgressBar控件的最小值为0
            progressBar1.Maximum = 10;              //设定ProgressBar控件的最大值为10
            progressBar1.MarqueeAnimationSpeed = 30; //设定ProgressBar控件进度块在进度栏中移动的时间段
            Counter.Start();                       //启动计时器
        }

        private void Counter_Tick(object sender, EventArgs e)
        {
            this.Hide();                           //隐藏本窗体;                                 //显示窗体MainForm
            myMainHome.Show();
            
            Counter.Stop();                                  //停止计时器
            this.Dispose();
        }
    }
}
