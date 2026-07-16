namespace 码料机
{
    partial class BOX
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tableFields = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tableFields.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.lblTitle, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tableFields, 0, 1);
            this.tableLayoutMain.Controls.Add(this.panelButtons, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(16, 14);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.Size = new System.Drawing.Size(408, 352);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(402, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "木箱大小";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableFields
            // 
            this.tableFields.ColumnCount = 2;
            this.tableFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableFields.Controls.Add(this.label5, 0, 0);
            this.tableFields.Controls.Add(this.textBox1, 1, 0);
            this.tableFields.Controls.Add(this.label2, 0, 1);
            this.tableFields.Controls.Add(this.textBox2, 1, 1);
            this.tableFields.Controls.Add(this.label3, 0, 2);
            this.tableFields.Controls.Add(this.textBox3, 1, 2);
            this.tableFields.Controls.Add(this.label4, 0, 3);
            this.tableFields.Controls.Add(this.textBox4, 1, 3);
            this.tableFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableFields.Location = new System.Drawing.Point(0, 40);
            this.tableFields.Margin = new System.Windows.Forms.Padding(0);
            this.tableFields.Name = "tableFields";
            this.tableFields.RowCount = 4;
            this.tableFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableFields.Size = new System.Drawing.Size(408, 248);
            this.tableFields.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.label5.Location = new System.Drawing.Point(3, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 24);
            this.label5.TabIndex = 0;
            this.label5.Text = "箱类型:";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.textBox1.Location = new System.Drawing.Point(113, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(292, 30);
            this.textBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.label2.Location = new System.Drawing.Point(3, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 24);
            this.label2.TabIndex = 2;
            this.label2.Text = "箱长:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.textBox2.Location = new System.Drawing.Point(113, 62);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(292, 30);
            this.textBox2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.label3.Location = new System.Drawing.Point(3, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "箱高:";
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.textBox3.Location = new System.Drawing.Point(113, 114);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(292, 30);
            this.textBox3.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.label4.Location = new System.Drawing.Point(3, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 24);
            this.label4.TabIndex = 6;
            this.label4.Text = "箱宽:";
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox4.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.textBox4.Location = new System.Drawing.Point(113, 166);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(292, 30);
            this.textBox4.TabIndex = 7;
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.button2);
            this.panelButtons.Controls.Add(this.button3);
            this.panelButtons.Controls.Add(this.button1);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelButtons.Location = new System.Drawing.Point(0, 296);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelButtons.Size = new System.Drawing.Size(408, 56);
            this.panelButtons.TabIndex = 2;
            this.panelButtons.WrapContents = false;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.button2.Location = new System.Drawing.Point(0, 11);
            this.button2.Margin = new System.Windows.Forms.Padding(0, 3, 8, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(96, 42);
            this.button2.TabIndex = 0;
            this.button2.Text = "删除";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.button3.Location = new System.Drawing.Point(104, 11);
            this.button3.Margin = new System.Windows.Forms.Padding(0, 3, 8, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(96, 42);
            this.button3.TabIndex = 1;
            this.button3.Text = "取消";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.button1.Location = new System.Drawing.Point(208, 11);
            this.button1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 42);
            this.button1.TabIndex = 2;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // BOX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(440, 380);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(456, 418);
            this.Name = "BOX";
            this.Padding = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "箱体设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BOX_FormClosing);
            this.Load += new System.EventHandler(this.BOX_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tableFields.ResumeLayout(false);
            this.tableFields.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel tableFields;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
    }
}
