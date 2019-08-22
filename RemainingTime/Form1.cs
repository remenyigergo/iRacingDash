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
using iRacingSdkWrapper.Bitfields;

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

                CheckFlag(e);
            }
            catch (Exception ex)
            {
                label1.Text = "00:00:00";
                Logger errorLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + iRacingDash.Form1.dateInString + "\\errorLog.txt");
                errorLogger.Log("Common RemainingTime Error", ex.Message);
            }
        }

        private void CheckFlag(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var sessionFlag = e.TelemetryInfo.SessionFlags.Value.ToString();


            var sessionFlags = (SessionFlags)Enum.Parse(typeof(SessionFlags), sessionFlag.Replace('|', ','));

            switch (sessionFlags)
            {
                case var t when t.HasFlag(SessionFlags.Yellow):
                case var t1 when t1.HasFlag(SessionFlags.Caution):
                case var t2 when t2.HasFlag(SessionFlags.CautionWaving):
                case var t3 when t3.HasFlag(SessionFlags.YellowWaving):
                    yellow_flag.Visible = true;
                    break;
                case var t4 when t4.HasFlag(SessionFlags.Checkered):
                    label1.Text = "CHECKERED";
                    goto default;
                case var t5 when t5.HasFlag(SessionFlags.White):
                    label1.Text = "FINAL LAP";
                    goto default;
                default:
                    yellow_flag.Visible = false;
                    break;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
