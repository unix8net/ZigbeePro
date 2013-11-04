using System;
using System.IO;
using System.Windows.Forms;

namespace AccleZigBee
{
    public partial class FileView : Form
    {
        public FileView()
        {
            InitializeComponent(); fillTree(treeView1);
        }
        public void update()
        {
            fillTree(treeView1);
        }
        private void fillTree(TreeView tv)
        {
            DirectoryInfo directory;
            string sCurPath = ""; // 重新清空 
            tv.Nodes.Clear();

            // 将硬盘上的所有的驱动器都列举出来 
           // foreach (char c in driveLetters)
            {
               // sCurPath = c + ":\\";
                sCurPath = System.AppDomain.CurrentDomain.BaseDirectory + @"Data\";
                //Console.WriteLine("{0}",sCurPath);
                try
                {
                    // 获得该路径的目录信息 
                    directory = new DirectoryInfo(sCurPath);

                    // 如果获得的目录信息正确，则将它添加到目录树视中 
                    if (directory.Exists == true)
                    {
                        TreeNode newNode = new TreeNode(directory.FullName);
                        tv.Nodes.Add(newNode); // 添加新的节点到根节点 
                        getSubDirs(newNode);
                        // 调用getSubDirs（）函数，检查该驱动器上的任何存在子目录 
                    }
                }
                catch (Exception doh)
                {
                    Console.WriteLine(doh.Message);
                }
            }
        }

        private void getSubDirs(TreeNode parent)
        {
            DirectoryInfo directory;
            try
            {
                // 如果还没有检查过这个文件夹，则检查之 
                if (parent.Nodes.Count == 0)
                {
                    directory = new DirectoryInfo(parent.FullPath);
                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        // 新建一个数节点，并添加到目录树视 
                        TreeNode newNode = new TreeNode(dir.Name);
                        parent.Nodes.Add(newNode);
                    }
                }

                foreach (TreeNode node in parent.Nodes)
                {
                    // 如果还没有检查过这个文件夹，则检查 
                    if (node.Nodes.Count == 0)
                    {
                        directory = new DirectoryInfo(node.FullPath);

                        // 检查该目录上的任何子目录 
                        foreach (DirectoryInfo dir in directory.GetDirectories())
                        {
                            // 新建一个数节点，并添加到目录树视 
                            TreeNode newNode = new TreeNode(dir.Name);
                            node.Nodes.Add(newNode);
                        }
                    }
                }
            }
            catch (Exception doh)
            {
                Console.WriteLine(doh.Message);
            }
        }

        private void fillListView(ListView lv, string strPath)
        {
            DirectoryInfo directory = new DirectoryInfo(strPath);
            lv.Items.Clear();
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                ListViewItem item = new ListViewItem(dir.Name);
                item.SubItems.Add(string.Empty);
                item.SubItems.Add("文件夹");
                item.SubItems.Add(string.Empty);
                lv.Items.Add(item);
            }
            foreach (FileInfo file in directory.GetFiles())
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.SubItems.Add((file.Length / 1024).ToString() + " KB");
                item.SubItems.Add(file.Extension + "文件");
                item.SubItems.Add(file.LastWriteTime.ToString());
                lv.Items.Add(item);
            }
        }

        private string fixPath(TreeNode node)
        {
            string sRet = "";
            try
            {
                sRet = node.FullPath;
                int index = sRet.IndexOf("\\\\");
                if (index > 1)
                {
                    sRet = node.FullPath.Remove(index, 1);
                }
            }
            catch (Exception doh)
            {
                Console.WriteLine(doh.Message);
            }
            return sRet;
        }

        private void treeView1_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            getSubDirs(e.Node); // 取得选择节点的子文件夹 
            textBox1.Text = fixPath(e.Node); // 更新文本框内容 
            folder = new DirectoryInfo(e.Node.FullPath); // 获得它的目录信息
            fillListView(listView1, fixPath(e.Node));
        }

        private void treeView1_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            getSubDirs(e.Node); // 取得选择节点的子文件夹 
            textBox1.Text = fixPath(e.Node); // 更新文本框内容 
            folder = new DirectoryInfo(e.Node.FullPath); // 获得它的目录信息 
        }
    }
}
