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
        private int minutes = 5;
        private int hours = 3;
        private int second = -2;
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

                //DateTimeOffset thisDate2 = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,hours, minutes, second, TimeSpan.Zero);
                
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
                air_temp_value.Text = e.TelemetryInfo.AirTemp.Value.ToString("##.#");
                track_tamp_value.Text = e.TelemetryInfo.TrackTemp.Value.ToString("##.#");
            }
            catch (Exception ex)
            {
                label1.Text = "00:00:00";
                Logger errorLogger = new Logger(iRacingDash.Form1.logPath + iRacingDash.Form1.dateInString + "\\errorLog.txt");
                errorLogger.Log("Common Time Error", ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
