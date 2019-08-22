namespace RemainingTime
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
            this.label1 = new System.Windows.Forms.Label();
            this.air_temp_text = new System.Windows.Forms.Label();
            this.air_temp_value = new System.Windows.Forms.Label();
            this.track_temp_text = new System.Windows.Forms.Label();
            this.track_tamp_value = new System.Windows.Forms.Label();
            this.yellow_flag = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.yellow_flag)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 33F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(-4, -6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(236, 64);
            this.label1.TabIndex = 0;
            this.label1.Text = "00:00:00";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // air_temp_text
            // 
            this.air_temp_text.AutoSize = true;
            this.air_temp_text.BackColor = System.Drawing.Color.Transparent;
            this.air_temp_text.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.air_temp_text.ForeColor = System.Drawing.Color.White;
            this.air_temp_text.Location = new System.Drawing.Point(13, 44);
            this.air_temp_text.Name = "air_temp_text";
            this.air_temp_text.Size = new System.Drawing.Size(64, 17);
            this.air_temp_text.TabIndex = 2;
            this.air_temp_text.Text = "Air Temp:";
            // 
            // air_temp_value
            // 
            this.air_temp_value.AutoSize = true;
            this.air_temp_value.BackColor = System.Drawing.Color.Transparent;
            this.air_temp_value.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.air_temp_value.ForeColor = System.Drawing.Color.White;
            this.air_temp_value.Location = new System.Drawing.Point(73, 44);
            this.air_temp_value.Name = "air_temp_value";
            this.air_temp_value.Size = new System.Drawing.Size(15, 17);
            this.air_temp_value.TabIndex = 3;
            this.air_temp_value.Text = "0";
            // 
            // track_temp_text
            // 
            this.track_temp_text.AutoSize = true;
            this.track_temp_text.BackColor = System.Drawing.Color.Transparent;
            this.track_temp_text.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.track_temp_text.ForeColor = System.Drawing.Color.White;
            this.track_temp_text.Location = new System.Drawing.Point(108, 45);
            this.track_temp_text.Name = "track_temp_text";
            this.track_temp_text.Size = new System.Drawing.Size(77, 17);
            this.track_temp_text.TabIndex = 4;
            this.track_temp_text.Text = "Track temp:";
            // 
            // track_tamp_value
            // 
            this.track_tamp_value.AutoSize = true;
            this.track_tamp_value.BackColor = System.Drawing.Color.Transparent;
            this.track_tamp_value.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.track_tamp_value.ForeColor = System.Drawing.Color.White;
            this.track_tamp_value.Location = new System.Drawing.Point(182, 46);
            this.track_tamp_value.Name = "track_tamp_value";
            this.track_tamp_value.Size = new System.Drawing.Size(15, 17);
            this.track_tamp_value.TabIndex = 5;
            this.track_tamp_value.Text = "0";
            // 
            // yellow_flag
            // 
            this.yellow_flag.Image = global::RemainingTime.Properties.Resources.yellow;
            this.yellow_flag.Location = new System.Drawing.Point(0, 61);
            this.yellow_flag.Name = "yellow_flag";
            this.yellow_flag.Size = new System.Drawing.Size(282, 58);
            this.yellow_flag.TabIndex = 6;
            this.yellow_flag.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::RemainingTime.Properties.Resources.time_v2;
            this.ClientSize = new System.Drawing.Size(221, 156);
            this.Controls.Add(this.yellow_flag);
            this.Controls.Add(this.track_tamp_value);
            this.Controls.Add(this.track_temp_text);
            this.Controls.Add(this.air_temp_value);
            this.Controls.Add(this.air_temp_text);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "RemainingTime";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.yellow_flag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label air_temp_text;
        private System.Windows.Forms.Label air_temp_value;
        private System.Windows.Forms.Label track_temp_text;
        private System.Windows.Forms.Label track_tamp_value;
        private System.Windows.Forms.PictureBox yellow_flag;
    }
}

