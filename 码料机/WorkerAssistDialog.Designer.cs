namespace 码料机
{
    partial class WorkerAssistDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 布局可在 Visual Studio 设计器中拖拽调整；业务逻辑在 WorkerAssistDialog.cs。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblPending = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnPlaced = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnFallen = new System.Windows.Forms.Button();
            this.btnReplan = new System.Windows.Forms.Button();
            this.lblRollbackPrefix = new System.Windows.Forms.Label();
            this.numRollback = new System.Windows.Forms.NumericUpDown();
            this.lblRollbackSuffix = new System.Windows.Forms.Label();
            this.btnRollback = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numRollback)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(528, 36);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "请现场确认";
            // 
            // lblProgress
            // 
            this.lblProgress.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblProgress.Location = new System.Drawing.Point(16, 52);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(528, 28);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "本箱进度";
            // 
            // lblPending
            // 
            this.lblPending.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.lblPending.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(83)))), ((int)(((byte)(9)))));
            this.lblPending.Location = new System.Drawing.Point(16, 82);
            this.lblPending.Name = "lblPending";
            this.lblPending.Size = new System.Drawing.Size(528, 56);
            this.lblPending.TabIndex = 2;
            this.lblPending.Text = "待确认";
            // 
            // lblHint
            // 
            this.lblHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblHint.Location = new System.Drawing.Point(16, 142);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(528, 72);
            this.lblHint.TabIndex = 3;
            this.lblHint.Text = "说明";
            // 
            // btnPlaced
            // 
            this.btnPlaced.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnPlaced.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlaced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlaced.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnPlaced.ForeColor = System.Drawing.Color.White;
            this.btnPlaced.Location = new System.Drawing.Point(16, 220);
            this.btnPlaced.Name = "btnPlaced";
            this.btnPlaced.Size = new System.Drawing.Size(252, 60);
            this.btnPlaced.TabIndex = 4;
            this.btnPlaced.Text = "上一件已放入";
            this.btnPlaced.UseVisualStyleBackColor = false;
            this.btnPlaced.Click += new System.EventHandler(this.btnPlaced_Click);
            // 
            // btnRetry
            // 
            this.btnRetry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.btnRetry.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRetry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRetry.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnRetry.ForeColor = System.Drawing.Color.White;
            this.btnRetry.Location = new System.Drawing.Point(280, 220);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(264, 60);
            this.btnRetry.TabIndex = 5;
            this.btnRetry.Text = "上一件未放入（重放）";
            this.btnRetry.UseVisualStyleBackColor = false;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnFallen
            // 
            this.btnFallen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnFallen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFallen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFallen.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnFallen.ForeColor = System.Drawing.Color.White;
            this.btnFallen.Location = new System.Drawing.Point(16, 290);
            this.btnFallen.Name = "btnFallen";
            this.btnFallen.Size = new System.Drawing.Size(252, 60);
            this.btnFallen.TabIndex = 6;
            this.btnFallen.Text = "有料倒了（先暂停）";
            this.btnFallen.UseVisualStyleBackColor = false;
            this.btnFallen.Click += new System.EventHandler(this.btnFallen_Click);
            // 
            // btnReplan
            // 
            this.btnReplan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnReplan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReplan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReplan.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnReplan.ForeColor = System.Drawing.Color.White;
            this.btnReplan.Location = new System.Drawing.Point(280, 290);
            this.btnReplan.Name = "btnReplan";
            this.btnReplan.Size = new System.Drawing.Size(264, 60);
            this.btnReplan.TabIndex = 7;
            this.btnReplan.Text = "箱子动了 / 换箱重来";
            this.btnReplan.UseVisualStyleBackColor = false;
            this.btnReplan.Click += new System.EventHandler(this.btnReplan_Click);
            // 
            // lblRollbackPrefix
            // 
            this.lblRollbackPrefix.AutoSize = true;
            this.lblRollbackPrefix.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.lblRollbackPrefix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblRollbackPrefix.Location = new System.Drawing.Point(16, 364);
            this.lblRollbackPrefix.Name = "lblRollbackPrefix";
            this.lblRollbackPrefix.Size = new System.Drawing.Size(82, 24);
            this.lblRollbackPrefix.TabIndex = 8;
            this.lblRollbackPrefix.Text = "回退到第";
            // 
            // numRollback
            // 
            this.numRollback.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.numRollback.Location = new System.Drawing.Point(104, 362);
            this.numRollback.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numRollback.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRollback.Name = "numRollback";
            this.numRollback.Size = new System.Drawing.Size(80, 28);
            this.numRollback.TabIndex = 9;
            this.numRollback.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblRollbackSuffix
            // 
            this.lblRollbackSuffix.AutoSize = true;
            this.lblRollbackSuffix.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.lblRollbackSuffix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblRollbackSuffix.Location = new System.Drawing.Point(190, 364);
            this.lblRollbackSuffix.Name = "lblRollbackSuffix";
            this.lblRollbackSuffix.Size = new System.Drawing.Size(163, 24);
            this.lblRollbackSuffix.TabIndex = 10;
            this.lblRollbackSuffix.Text = "件（已确认件数）";
            // 
            // btnRollback
            // 
            this.btnRollback.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.btnRollback.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRollback.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRollback.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnRollback.ForeColor = System.Drawing.Color.White;
            this.btnRollback.Location = new System.Drawing.Point(16, 400);
            this.btnRollback.Name = "btnRollback";
            this.btnRollback.Size = new System.Drawing.Size(252, 60);
            this.btnRollback.TabIndex = 11;
            this.btnRollback.Text = "确认回退";
            this.btnRollback.UseVisualStyleBackColor = false;
            this.btnRollback.Click += new System.EventHandler(this.btnRollback_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(280, 400);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(264, 60);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "暂不处理";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // WorkerAssistDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(560, 520);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRollback);
            this.Controls.Add(this.lblRollbackSuffix);
            this.Controls.Add(this.numRollback);
            this.Controls.Add(this.lblRollbackPrefix);
            this.Controls.Add(this.btnReplan);
            this.Controls.Add(this.btnFallen);
            this.Controls.Add(this.btnRetry);
            this.Controls.Add(this.btnPlaced);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblPending);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkerAssistDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "现场放料确认";
            ((System.ComponentModel.ISupportInitialize)(this.numRollback)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblPending;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnPlaced;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnFallen;
        private System.Windows.Forms.Button btnReplan;
        private System.Windows.Forms.Label lblRollbackPrefix;
        private System.Windows.Forms.NumericUpDown numRollback;
        private System.Windows.Forms.Label lblRollbackSuffix;
        private System.Windows.Forms.Button btnRollback;
        private System.Windows.Forms.Button btnCancel;
    }
}
