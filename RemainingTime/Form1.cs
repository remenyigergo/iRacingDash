using iRacingSdkWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemainingTime
{
    public partial class Form1 : Form
    {

        private SdkWrapper wrapper;
        public Form1()
        {
            InitializeComponent();

            

            wrapper = new SdkWrapper();
            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 60;
            wrapper.TelemetryUpdated += OnTelemetryUpdated;
        }

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var seconds = (int)e.TelemetryInfo.SessionTimeRemain.Value;
            var hours = seconds / 60 / 60;
            var minutes= (int)((float)(seconds-(hours*60*60)) / (float)60.0f);
            var second = seconds % 60;

            DateTimeOffset thisDate2 = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, second, TimeSpan.Zero);
            string s = string.Format("{0:HH:mm:ss}", thisDate2);

            label1.Text = s;
        }
    }
}
