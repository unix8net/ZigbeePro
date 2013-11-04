using System;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class Notify : Form
    {
        public Notify(string title)
        {
            InitializeComponent();
            this.label1.Text = title;
            this.FormBorderStyle = FormBorderStyle.None;
            timer1.Interval = 2000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Dispose();
        }
    }
}
