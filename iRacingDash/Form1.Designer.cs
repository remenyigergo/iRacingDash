namespace iRacingDash
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Speed_title = new System.Windows.Forms.Label();
            this.Speed_value = new System.Windows.Forms.Label();
            this.Laps_estimate_title = new System.Windows.Forms.Label();
            this.Laps_estimate_value = new System.Windows.Forms.Label();
            this.Fuel_remain_title = new System.Windows.Forms.Label();
            this.Fuel_remain_value = new System.Windows.Forms.Label();
            this.Automatic_value = new System.Windows.Forms.Label();
            this.Laptime_title = new System.Windows.Forms.Label();
            this.Laptime_value = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.gear = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.led1_1 = new System.Windows.Forms.PictureBox();
            this.led1_2 = new System.Windows.Forms.PictureBox();
            this.led1_3 = new System.Windows.Forms.PictureBox();
            this.led1_4 = new System.Windows.Forms.PictureBox();
            this.led1_5 = new System.Windows.Forms.PictureBox();
            this.led1_6 = new System.Windows.Forms.PictureBox();
            this.led2_1 = new System.Windows.Forms.PictureBox();
            this.led2_2 = new System.Windows.Forms.PictureBox();
            this.led2_3 = new System.Windows.Forms.PictureBox();
            this.led3_1 = new System.Windows.Forms.PictureBox();
            this.led3_2 = new System.Windows.Forms.PictureBox();
            this.led3_3 = new System.Windows.Forms.PictureBox();
            this.rpm = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_3)).BeginInit();
            this.SuspendLayout();
            // 
            // Speed_title
            // 
            this.Speed_title.AutoSize = true;
            this.Speed_title.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Speed_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Speed_title.Location = new System.Drawing.Point(28, 40);
            this.Speed_title.Name = "Speed_title";
            this.Speed_title.Size = new System.Drawing.Size(51, 19);
            this.Speed_title.TabIndex = 0;
            this.Speed_title.Text = "Speed";
            // 
            // Speed_value
            // 
            this.Speed_value.AutoSize = true;
            this.Speed_value.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Speed_value.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Speed_value.Location = new System.Drawing.Point(35, 13);
            this.Speed_value.Name = "Speed_value";
            this.Speed_value.Size = new System.Drawing.Size(44, 22);
            this.Speed_value.TabIndex = 1;
            this.Speed_value.Text = "N/A";
            // 
            // Laps_estimate_title
            // 
            this.Laps_estimate_title.AutoSize = true;
            this.Laps_estimate_title.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Laps_estimate_title.ForeColor = System.Drawing.Color.Blue;
            this.Laps_estimate_title.Location = new System.Drawing.Point(254, 270);
            this.Laps_estimate_title.Name = "Laps_estimate_title";
            this.Laps_estimate_title.Size = new System.Drawing.Size(99, 19);
            this.Laps_estimate_title.TabIndex = 2;
            this.Laps_estimate_title.Text = "Laps Estimate";
            // 
            // Laps_estimate_value
            // 
            this.Laps_estimate_value.AutoSize = true;
            this.Laps_estimate_value.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Laps_estimate_value.ForeColor = System.Drawing.Color.Blue;
            this.Laps_estimate_value.Location = new System.Drawing.Point(275, 242);
            this.Laps_estimate_value.Name = "Laps_estimate_value";
            this.Laps_estimate_value.Size = new System.Drawing.Size(44, 22);
            this.Laps_estimate_value.TabIndex = 3;
            this.Laps_estimate_value.Text = "N/A";
            // 
            // Fuel_remain_title
            // 
            this.Fuel_remain_title.AutoSize = true;
            this.Fuel_remain_title.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Fuel_remain_title.ForeColor = System.Drawing.Color.Blue;
            this.Fuel_remain_title.Location = new System.Drawing.Point(253, 204);
            this.Fuel_remain_title.Name = "Fuel_remain_title";
            this.Fuel_remain_title.Size = new System.Drawing.Size(90, 19);
            this.Fuel_remain_title.TabIndex = 4;
            this.Fuel_remain_title.Text = "Fuel Remain";
            // 
            // Fuel_remain_value
            // 
            this.Fuel_remain_value.AutoSize = true;
            this.Fuel_remain_value.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Fuel_remain_value.ForeColor = System.Drawing.Color.Blue;
            this.Fuel_remain_value.Location = new System.Drawing.Point(274, 174);
            this.Fuel_remain_value.Name = "Fuel_remain_value";
            this.Fuel_remain_value.Size = new System.Drawing.Size(44, 22);
            this.Fuel_remain_value.TabIndex = 5;
            this.Fuel_remain_value.Text = "N/A";
            // 
            // Automatic_value
            // 
            this.Automatic_value.AutoSize = true;
            this.Automatic_value.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Automatic_value.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.Automatic_value.Location = new System.Drawing.Point(35, 7);
            this.Automatic_value.Name = "Automatic_value";
            this.Automatic_value.Size = new System.Drawing.Size(44, 22);
            this.Automatic_value.TabIndex = 7;
            this.Automatic_value.Text = "N/A";
            // 
            // Laptime_title
            // 
            this.Laptime_title.AutoSize = true;
            this.Laptime_title.Font = new System.Drawing.Font("Microsoft YaHei", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Laptime_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Laptime_title.Location = new System.Drawing.Point(25, 39);
            this.Laptime_title.Name = "Laptime_title";
            this.Laptime_title.Size = new System.Drawing.Size(70, 19);
            this.Laptime_title.TabIndex = 8;
            this.Laptime_title.Text = "Laptime";
            // 
            // Laptime_value
            // 
            this.Laptime_value.AutoSize = true;
            this.Laptime_value.Font = new System.Drawing.Font("Microsoft YaHei", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Laptime_value.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Laptime_value.Location = new System.Drawing.Point(24, 115);
            this.Laptime_value.Name = "Laptime_value";
            this.Laptime_value.Size = new System.Drawing.Size(82, 26);
            this.Laptime_value.TabIndex = 9;
            this.Laptime_value.Text = "0:00:00";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.Laptime_title);
            this.panel1.Location = new System.Drawing.Point(6, 105);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(126, 58);
            this.panel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.Speed_title);
            this.panel2.Controls.Add(this.Speed_value);
            this.panel2.Location = new System.Drawing.Point(6, 169);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(126, 58);
            this.panel2.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.label1.Location = new System.Drawing.Point(18, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 19);
            this.label1.TabIndex = 12;
            this.label1.Text = "Automatic";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.Automatic_value);
            this.panel3.Location = new System.Drawing.Point(6, 233);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(126, 58);
            this.panel3.TabIndex = 12;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Location = new System.Drawing.Point(239, 234);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(123, 58);
            this.panel5.TabIndex = 13;
            // 
            // panel6
            // 
            this.panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel6.Controls.Add(this.gear);
            this.panel6.Location = new System.Drawing.Point(138, 161);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(95, 131);
            this.panel6.TabIndex = 13;
            // 
            // gear
            // 
            this.gear.AutoSize = true;
            this.gear.Font = new System.Drawing.Font("Microsoft YaHei", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.gear.ForeColor = System.Drawing.Color.White;
            this.gear.Location = new System.Drawing.Point(-17, 0);
            this.gear.Name = "gear";
            this.gear.Size = new System.Drawing.Size(136, 128);
            this.gear.TabIndex = 0;
            this.gear.Text = "N";
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Location = new System.Drawing.Point(239, 171);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(123, 58);
            this.panel4.TabIndex = 12;
            // 
            // led1_1
            // 
            this.led1_1.Image = ((System.Drawing.Image)(resources.GetObject("led1_1.Image")));
            this.led1_1.Location = new System.Drawing.Point(12, 12);
            this.led1_1.Name = "led1_1";
            this.led1_1.Size = new System.Drawing.Size(24, 45);
            this.led1_1.TabIndex = 14;
            this.led1_1.TabStop = false;
            // 
            // led1_2
            // 
            this.led1_2.Image = ((System.Drawing.Image)(resources.GetObject("led1_2.Image")));
            this.led1_2.Location = new System.Drawing.Point(39, 12);
            this.led1_2.Name = "led1_2";
            this.led1_2.Size = new System.Drawing.Size(24, 45);
            this.led1_2.TabIndex = 15;
            this.led1_2.TabStop = false;
            // 
            // led1_3
            // 
            this.led1_3.Image = ((System.Drawing.Image)(resources.GetObject("led1_3.Image")));
            this.led1_3.Location = new System.Drawing.Point(66, 12);
            this.led1_3.Name = "led1_3";
            this.led1_3.Size = new System.Drawing.Size(24, 45);
            this.led1_3.TabIndex = 16;
            this.led1_3.TabStop = false;
            // 
            // led1_4
            // 
            this.led1_4.Image = ((System.Drawing.Image)(resources.GetObject("led1_4.Image")));
            this.led1_4.Location = new System.Drawing.Point(92, 12);
            this.led1_4.Name = "led1_4";
            this.led1_4.Size = new System.Drawing.Size(24, 45);
            this.led1_4.TabIndex = 17;
            this.led1_4.TabStop = false;
            // 
            // led1_5
            // 
            this.led1_5.Image = ((System.Drawing.Image)(resources.GetObject("led1_5.Image")));
            this.led1_5.Location = new System.Drawing.Point(119, 12);
            this.led1_5.Name = "led1_5";
            this.led1_5.Size = new System.Drawing.Size(24, 45);
            this.led1_5.TabIndex = 18;
            this.led1_5.TabStop = false;
            // 
            // led1_6
            // 
            this.led1_6.Image = ((System.Drawing.Image)(resources.GetObject("led1_6.Image")));
            this.led1_6.Location = new System.Drawing.Point(146, 12);
            this.led1_6.Name = "led1_6";
            this.led1_6.Size = new System.Drawing.Size(24, 45);
            this.led1_6.TabIndex = 19;
            this.led1_6.TabStop = false;
            // 
            // led2_1
            // 
            this.led2_1.Image = ((System.Drawing.Image)(resources.GetObject("led2_1.Image")));
            this.led2_1.Location = new System.Drawing.Point(176, 12);
            this.led2_1.Name = "led2_1";
            this.led2_1.Size = new System.Drawing.Size(24, 45);
            this.led2_1.TabIndex = 20;
            this.led2_1.TabStop = false;
            // 
            // led2_2
            // 
            this.led2_2.Image = ((System.Drawing.Image)(resources.GetObject("led2_2.Image")));
            this.led2_2.Location = new System.Drawing.Point(204, 12);
            this.led2_2.Name = "led2_2";
            this.led2_2.Size = new System.Drawing.Size(24, 45);
            this.led2_2.TabIndex = 21;
            this.led2_2.TabStop = false;
            // 
            // led2_3
            // 
            this.led2_3.Image = ((System.Drawing.Image)(resources.GetObject("led2_3.Image")));
            this.led2_3.Location = new System.Drawing.Point(232, 12);
            this.led2_3.Name = "led2_3";
            this.led2_3.Size = new System.Drawing.Size(24, 45);
            this.led2_3.TabIndex = 22;
            this.led2_3.TabStop = false;
            // 
            // led3_1
            // 
            this.led3_1.Image = ((System.Drawing.Image)(resources.GetObject("led3_1.Image")));
            this.led3_1.Location = new System.Drawing.Point(262, 12);
            this.led3_1.Name = "led3_1";
            this.led3_1.Size = new System.Drawing.Size(24, 45);
            this.led3_1.TabIndex = 23;
            this.led3_1.TabStop = false;
            // 
            // led3_2
            // 
            this.led3_2.Image = ((System.Drawing.Image)(resources.GetObject("led3_2.Image")));
            this.led3_2.Location = new System.Drawing.Point(289, 12);
            this.led3_2.Name = "led3_2";
            this.led3_2.Size = new System.Drawing.Size(24, 45);
            this.led3_2.TabIndex = 24;
            this.led3_2.TabStop = false;
            // 
            // led3_3
            // 
            this.led3_3.Image = ((System.Drawing.Image)(resources.GetObject("led3_3.Image")));
            this.led3_3.Location = new System.Drawing.Point(317, 12);
            this.led3_3.Name = "led3_3";
            this.led3_3.Size = new System.Drawing.Size(24, 45);
            this.led3_3.TabIndex = 25;
            this.led3_3.TabStop = false;
            // 
            // rpm
            // 
            this.rpm.AutoSize = true;
            this.rpm.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.rpm.Location = new System.Drawing.Point(176, 105);
            this.rpm.Name = "rpm";
            this.rpm.Size = new System.Drawing.Size(35, 13);
            this.rpm.TabIndex = 26;
            this.rpm.Text = "label2";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(370, 304);
            this.Controls.Add(this.rpm);
            this.Controls.Add(this.led3_3);
            this.Controls.Add(this.led3_2);
            this.Controls.Add(this.led3_1);
            this.Controls.Add(this.led2_3);
            this.Controls.Add(this.led2_2);
            this.Controls.Add(this.led2_1);
            this.Controls.Add(this.led1_6);
            this.Controls.Add(this.led1_5);
            this.Controls.Add(this.led1_4);
            this.Controls.Add(this.led1_3);
            this.Controls.Add(this.led1_2);
            this.Controls.Add(this.led1_1);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.Laptime_value);
            this.Controls.Add(this.Fuel_remain_value);
            this.Controls.Add(this.Fuel_remain_title);
            this.Controls.Add(this.Laps_estimate_value);
            this.Controls.Add(this.Laps_estimate_title);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(750, 650);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.TopMost = true;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led1_6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3_3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Speed_title;
        private System.Windows.Forms.Label Speed_value;
        private System.Windows.Forms.Label Laps_estimate_title;
        private System.Windows.Forms.Label Laps_estimate_value;
        private System.Windows.Forms.Label Fuel_remain_title;
        private System.Windows.Forms.Label Fuel_remain_value;
        private System.Windows.Forms.Label Automatic_value;
        private System.Windows.Forms.Label Laptime_title;
        private System.Windows.Forms.Label Laptime_value;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label gear;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.PictureBox led1_1;
        private System.Windows.Forms.PictureBox led1_2;
        private System.Windows.Forms.PictureBox led1_3;
        private System.Windows.Forms.PictureBox led1_4;
        private System.Windows.Forms.PictureBox led1_5;
        private System.Windows.Forms.PictureBox led1_6;
        private System.Windows.Forms.PictureBox led2_1;
        private System.Windows.Forms.PictureBox led2_2;
        private System.Windows.Forms.PictureBox led2_3;
        private System.Windows.Forms.PictureBox led3_1;
        private System.Windows.Forms.PictureBox led3_2;
        private System.Windows.Forms.PictureBox led3_3;
        private System.Windows.Forms.Label rpm;
    }
}

