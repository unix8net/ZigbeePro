using System.IO;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Help\ReadMe_1.txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Help\ReadMe_2.txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Help\ReadMe_3.txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Help\ReadMe_4.txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Help\ReadMe_5.txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }
    }
}
