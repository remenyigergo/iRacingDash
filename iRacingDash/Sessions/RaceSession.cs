using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacingSdkWrapper;
using iRacingSdkWrapper.Bitfields;

namespace iRacingDash.Sessions
{
    class RaceSession : Session
    {
        private bool _raceStarted;
        private bool _raceFinished;

        public RaceSession(int nonRtFps, Form1 form, SdkWrapper wrapper, Dash dash) : base(nonRtFps, form, wrapper, dash)
        {
            
        }

        public override void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void NonRealtimeCalculations(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void FlashFlags(SdkWrapper.TelemetryUpdatedEventArgs e)
        {

            var sessionFlag = e.TelemetryInfo.SessionFlags.Value?.ToString();
            if (sessionFlag == null) return;

            var sessionFlags = (SessionFlags)Enum.Parse(typeof(SessionFlags), sessionFlag.Replace('|', ','));

            switch (sessionFlags)
            {
                case var t when t.HasFlag(SessionFlags.Repair):
                    LightPanel(dashForm.warning_panel, Color.Black);
                    break;
                case var t when t.HasFlag(SessionFlags.Blue):
                    LightPanel(dashForm.warning_panel, Color.Blue);
                    break;
                case var t when t.HasFlag(SessionFlags.Yellow):
                case var t1 when t1.HasFlag(SessionFlags.Caution):
                case var t2 when t2.HasFlag(SessionFlags.CautionWaving):
                case var t3 when t3.HasFlag(SessionFlags.YellowWaving):
                    LightPanel(dashForm.warning_panel, Color.Yellow);
                    break;
                case var t when t.HasFlag(SessionFlags.Green):
                    raceStarted = true;
                    raceStartLap = currentLap;
                    LightPanel(dashForm.warning_panel, Color.Green);
                    break;
                case var t1 when t1.HasFlag(SessionFlags.GreenHeld):
                case var t2 when t2.HasFlag(SessionFlags.OneLapToGreen):
                    LightPanel(dashForm.warning_panel, Color.Green);
                    break;
                case var t when t.HasFlag(SessionFlags.White):
                    LightPanel(dashForm.warning_panel, Color.White);
                    break;
                default:
                    dashForm.warning_panel.BackColor = Color.Transparent;
                    break;
            }
        }

        //private void CarLeftRight(SdkWrapper.TelemetryUpdatedEventArgs e)
        //{
        //    var carLeftRight = _wrapper.GetData("CarLeftRight").ToString();


        //    switch (Int32.Parse(carLeftRight))
        //    {
        //        case 2:
        //            carLeftPanel.Visible = true;
        //            break;
        //        case 3:
        //            carRightPanel.Visible = true;
        //            break;
        //        case 5:
        //            carLeftPanel.Visible = true;
        //            carRightPanel.Visible = true;
        //            break;
        //        default:
        //            carLeftPanel.Visible = false;
        //            carRightPanel.Visible = false;
        //            break;
        //    }

        //}
    }
}
