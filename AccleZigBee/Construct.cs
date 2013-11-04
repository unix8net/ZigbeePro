using System;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class Construct : Form
    {
        public Construct()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            timer1.Interval = 1500;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Dispose();
        }
    }
}
