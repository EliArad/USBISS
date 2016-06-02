namespace PASimpleApp
{
    partial class ModuleInfoForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblSerialNum = new System.Windows.Forms.Label();
            this.lblHwVersion = new System.Windows.Forms.Label();
            this.lblSWVersion = new System.Windows.Forms.Label();
            this.lblI2CAddress = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblName);
            this.groupBox1.Controls.Add(this.lblSerialNum);
            this.groupBox1.Controls.Add(this.lblHwVersion);
            this.groupBox1.Controls.Add(this.lblSWVersion);
            this.groupBox1.Controls.Add(this.lblI2CAddress);
            this.groupBox1.Location = new System.Drawing.Point(41, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(158, 218);
            this.groupBox1.TabIndex = 54;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(5, 165);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "label4";
            // 
            // lblSerialNum
            // 
            this.lblSerialNum.AutoSize = true;
            this.lblSerialNum.Location = new System.Drawing.Point(6, 131);
            this.lblSerialNum.Name = "lblSerialNum";
            this.lblSerialNum.Size = new System.Drawing.Size(35, 13);
            this.lblSerialNum.TabIndex = 0;
            this.lblSerialNum.Text = "label4";
            // 
            // lblHwVersion
            // 
            this.lblHwVersion.AutoSize = true;
            this.lblHwVersion.Location = new System.Drawing.Point(6, 97);
            this.lblHwVersion.Name = "lblHwVersion";
            this.lblHwVersion.Size = new System.Drawing.Size(35, 13);
            this.lblHwVersion.TabIndex = 0;
            this.lblHwVersion.Text = "label4";
            // 
            // lblSWVersion
            // 
            this.lblSWVersion.AutoSize = true;
            this.lblSWVersion.Location = new System.Drawing.Point(6, 63);
            this.lblSWVersion.Name = "lblSWVersion";
            this.lblSWVersion.Size = new System.Drawing.Size(35, 13);
            this.lblSWVersion.TabIndex = 0;
            this.lblSWVersion.Text = "label4";
            // 
            // lblI2CAddress
            // 
            this.lblI2CAddress.AutoSize = true;
            this.lblI2CAddress.Location = new System.Drawing.Point(6, 30);
            this.lblI2CAddress.Name = "lblI2CAddress";
            this.lblI2CAddress.Size = new System.Drawing.Size(64, 13);
            this.lblI2CAddress.TabIndex = 0;
            this.lblI2CAddress.Text = "I2C Address";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(74, 250);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 55;
            this.button1.Text = "&Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ModuleInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 285);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "ModuleInfoForm";
            this.Text = "ModuleInfoForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblSerialNum;
        private System.Windows.Forms.Label lblHwVersion;
        private System.Windows.Forms.Label lblSWVersion;
        private System.Windows.Forms.Label lblI2CAddress;
        private System.Windows.Forms.Button button1;
    }
}