namespace AccleZigBee
{
    partial class Start
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxStopBit = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxDataBit = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboPortName = new System.Windows.Forms.ComboBox();
            this.comboBoxParity = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBaudrate = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comm = new System.IO.Ports.SerialPort(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ok = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(175, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 28);
            this.label8.TabIndex = 34;
            this.label8.Text = "串口配置";
            // 
            // comboBoxStopBit
            // 
            this.comboBoxStopBit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxStopBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStopBit.FormattingEnabled = true;
            this.comboBoxStopBit.Items.AddRange(new object[] {
            "1",
            "2"});
            this.comboBoxStopBit.Location = new System.Drawing.Point(162, 166);
            this.comboBoxStopBit.Name = "comboBoxStopBit";
            this.comboBoxStopBit.Size = new System.Drawing.Size(135, 20);
            this.comboBoxStopBit.TabIndex = 31;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(107, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 30;
            this.label5.Text = "停止位:";
            // 
            // comboBoxDataBit
            // 
            this.comboBoxDataBit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxDataBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataBit.FormattingEnabled = true;
            this.comboBoxDataBit.Items.AddRange(new object[] {
            "8",
            "7",
            "6"});
            this.comboBoxDataBit.Location = new System.Drawing.Point(162, 110);
            this.comboBoxDataBit.Name = "comboBoxDataBit";
            this.comboBoxDataBit.Size = new System.Drawing.Size(135, 20);
            this.comboBoxDataBit.TabIndex = 29;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(107, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 28;
            this.label4.Text = "数据位:";
            // 
            // comboPortName
            // 
            this.comboPortName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPortName.FormattingEnabled = true;
            this.comboPortName.Location = new System.Drawing.Point(162, 54);
            this.comboPortName.Name = "comboPortName";
            this.comboPortName.Size = new System.Drawing.Size(135, 20);
            this.comboPortName.Sorted = true;
            this.comboPortName.TabIndex = 23;
            // 
            // comboBoxParity
            // 
            this.comboBoxParity.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParity.FormattingEnabled = true;
            this.comboBoxParity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.comboBoxParity.Location = new System.Drawing.Point(162, 138);
            this.comboBoxParity.Name = "comboBoxParity";
            this.comboBoxParity.Size = new System.Drawing.Size(135, 20);
            this.comboBoxParity.TabIndex = 27;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(107, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "串口名:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(107, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 26;
            this.label3.Text = "效验位:";
            // 
            // comboBaudrate
            // 
            this.comboBaudrate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBaudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBaudrate.FormattingEnabled = true;
            this.comboBaudrate.Items.AddRange(new object[] {
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBaudrate.Location = new System.Drawing.Point(162, 82);
            this.comboBaudrate.Name = "comboBaudrate";
            this.comboBaudrate.Size = new System.Drawing.Size(135, 20);
            this.comboBaudrate.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(107, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 24;
            this.label2.Text = "波特率:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timerCheckComPorts_Tick);
            // 
            // ok
            // 
            this.ok.BackgroundImage = global::AccleZigBee.Properties.Resources.start;
            this.ok.Location = new System.Drawing.Point(185, 210);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(82, 34);
            this.ok.TabIndex = 37;
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("黑体", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(419, 248);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 38;
            this.label6.Text = "V5.0";
            // 
            // Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::AccleZigBee.Properties.Resources.back;
            this.ClientSize = new System.Drawing.Size(452, 264);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.comboBoxStopBit);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBoxDataBit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboPortName);
            this.Controls.Add(this.comboBoxParity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBaudrate);
            this.Controls.Add(this.label2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Start";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ZigBee加速度表网控制中心登陆";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Start_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxStopBit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxDataBit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboPortName;
        private System.Windows.Forms.ComboBox comboBoxParity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBaudrate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ok;
        private System.IO.Ports.SerialPort comm;
        private System.Windows.Forms.Timer timer1;
        private MainHome mainHome;
        private System.Windows.Forms.Label label6;
    }
}