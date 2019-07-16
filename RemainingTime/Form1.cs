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
using iRacingDash;

namespace RemainingTime
{
    public partial class Form1 : Form
    {
        private int minutes = 0;
        private int hours = 0;
        private int second = 0;
        private static string time = "00:00:00";

        private SdkWrapper wrapper;
        public Form1()
        {
            InitializeComponent();
            wrapper = new SdkWrapper();
            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 5;
            wrapper.TelemetryUpdated += OnTelemetryUpdated;

            time = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, second);
        }

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            try
            {
                var seconds = (int) e.TelemetryInfo.SessionTimeRemain.Value;
                hours = seconds / 60 / 60;
                minutes = (int) ((float) (seconds - (hours * 60 * 60)) / (float) 60.0f);
                second = seconds % 60;

                if (hours < 0 || minutes < 0 || second < 0)
                {
                    time = string.Format("-{0:00}:{1:00}:{2:00}", Math.Abs(hours), Math.Abs(minutes),
                        Math.Abs(second));
                }
                else
                {
                    time = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, second);
                }

                label1.Text = time;
                air_temp_value.Text = string.Format("{0:0.0}", e.TelemetryInfo.AirTemp.Value);
                track_tamp_value.Text = string.Format("{0:0.0}", e.TelemetryInfo.TrackTemp.Value);
            }
            catch (Exception ex)
            {
                label1.Text = "00:00:00";
                Logger errorLogger = new Logger(iRacingDash.Form1.logPath +"\\"+ iRacingDash.Form1.dateInString + "\\errorLog.txt");
                errorLogger.Log("Common Time Error", ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
