using iRacingSdkWrapper;
using iRacingSdkWrapper.Bitfields;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iRSDKSharp;

namespace iRacingDash.Sessions
{
    public abstract class Session
    {
        //SDK + dash
        protected SdkWrapper sessionWrapper;
        protected iRacingSDK sessionSdk;
        protected Form1 sessionForm;
        protected Dash sessionDash;

        //Time
        private static DateTime now = DateTime.Now;
        public static string dateInString = now.ToString().Replace(' ', '-').Replace('/', '-').Replace(':', '-');

        //Loggers
        protected Logger newLapLogger;
        protected Logger fuelUsageLogger;
        protected Logger fuelComputeLogger;
        protected Logger errorLogger;

        //Panels
        protected Panel settingsPanel;
        protected Label settingLabelTitle;
        protected Label settingLabelValue;
        protected Label idleClock;

        //Laps
        protected double lapTime;
        protected string lapTimeString;
        protected float currentLap;
        protected bool raceStarted;
        protected float raceStartLap;

        //Laptimes
        protected List<double> laptimes = new List<double>();

        //Fuel
        protected float fuelLapStart;
        protected float fuelLevel;
        protected bool initForFuel = true;
        protected float maxFuelOfCar = 0;
        protected List<double> fuelUsagePerLap = new List<double>();

        //Engine
        protected int engineWarning;
        protected int gearValue = 0;

        //Car settings
        protected int settingPanelFps = 2;
        protected int settingPanelFpsCounter = -1;

        protected int sessionNumber;
        protected int sessionNumberTemp = -1;

        protected int subSessionNumber;
        protected int subSessionNumberTemp = -1;

        //Commons
        protected int position;
        protected double remainingTime;
        protected double remainingTimeTemp;
        protected bool raceOver = false;

        protected int fpsCounter = 0;
        protected int flashingFps = 2;
        protected int flashingFpsCounter = 0;
        protected int NonRTCalculationFPS;
        protected float lapDist = 0;
        protected int lapCount = 0;
        protected int lapCountTemp = -1;
        protected float lapCountPrevious = 0;
        protected float trackLength = -9999;
        protected bool initLapTime = true;
        protected string sessionType;
        protected string raceWeek;
        protected string eventType;
        protected string trackName;
        protected bool isDriverInCar;
        

        //Settings on car
        protected int boost;
        protected int boostTemp = -1;

        protected int tractionControl1;
        protected int tractionControl1Temp = -1;

        protected int tractionControl2;
        protected int tractionControl2Temp = -1;

        protected float brakeBias;
        protected float brakeBiasTemp = -1;

        protected int engineMap;
        protected int engineMapTemp = -1;

        //Idx vars
        protected int DriverCarIdx = -9999;




        public Session(int _nonRtFps, Form1 _form, SdkWrapper _sessionWrapper, Dash _sessionDash)
        {
            NonRTCalculationFPS = _nonRtFps;
            sessionWrapper = _sessionWrapper;
            sessionForm = _form;
            sessionDash = _sessionDash;
        }


        public abstract void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e);

        public abstract void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e);

        protected abstract void NonRealtimeCalculations(SdkWrapper.TelemetryUpdatedEventArgs e);

        protected abstract void FlashFlags(SdkWrapper.TelemetryUpdatedEventArgs e);

        protected void WarningFlashes(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            flashingFpsCounter = 0;

            if (fuelLevel < 5)
                sessionForm.panel4.BackColor = Color.DarkRed;
            else if (fuelLevel < 10)
                LightPanel(sessionForm.panel4, Color.DarkRed);
            else
                sessionForm.panel4.BackColor = Color.Transparent;

            if (engineWarning == 30 || engineWarning == 14)
            {
                if (sessionForm.engine_value.Visible)
                    sessionForm.engine_value.Visible = false;
                else
                    sessionForm.engine_value.Visible = true;
            }
        }

        

        protected void LightPanel(Panel panel, Color color)
        {
            if (panel.BackColor != Color.Transparent)
            {
                panel.BackColor = Color.Transparent;
            }
            else
            {
                panel.BackColor = color;
            }
        }

        protected void InPitsV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            engineWarning = Int32.Parse(sessionWrapper.GetData("EngineWarnings").ToString());

            var onPitRoad = Convert.ToBoolean(sessionWrapper.GetData("OnPitRoad"));
            var ontrack = Convert.ToBoolean(e.TelemetryInfo.IsOnTrack.Value);

            if ((onPitRoad && initForFuel) || (ontrack && initForFuel))
                initForFuel = false;

            if (engineWarning == 16 || engineWarning == 48) //16 = pit limiter
            {
                sessionForm.Pit_Limiter_title.Visible = onPitRoad ? true : false;
                sessionForm.speed_limiter.Visible = true;
                sessionForm.Pit_limiter_background_panel.Visible = true;
            }
            else
            {
                sessionForm.Pit_Limiter_title.Visible = onPitRoad ? true : false;
                sessionForm.Pit_Limiter_title.Visible = false;
                sessionForm.Pit_limiter_background_panel.Visible = false;
            }
        }

        protected void CalculateGear(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            gearValue = e.TelemetryInfo.Gear.Value;
            if (gearValue == -1)
                sessionForm.gear.Text = "R";
            else if (gearValue == 0)
                sessionForm.gear.Text = "N";
            else sessionForm.gear.Text = gearValue.ToString();
        }

        protected void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var deltaObject = sessionWrapper.GetData("LapDeltaToSessionBestLap");
            var deltaInt = Convert.ToDouble(deltaObject);

            if (deltaInt > 0 && deltaInt < 99.99)
            {
                sessionForm.delta_panel.BackColor = Color.Firebrick;
                sessionForm.Delta_value.Text = string.Format("{0:+0.00}", deltaInt);
            }
            else if (deltaInt < 0 && deltaInt > -99.99)
            {
                sessionForm.delta_panel.BackColor = Color.Green;
                sessionForm.Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt == 0)
            {
                sessionForm.delta_panel.BackColor = Color.FromArgb(255, 40, 40, 40);
                sessionForm.Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt >= 99.99)
            {
                sessionForm.delta_panel.BackColor = Color.Firebrick;
                sessionForm.Delta_value.Text = "+99.99";
            }
            else if (deltaInt <= -99.99)
            {
                sessionForm.delta_panel.BackColor = Color.Green;
                sessionForm.Delta_value.Text = "-99.99";
            }
        }

        protected void UpdateLapTimeV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapObject = sessionWrapper.GetData("LapCurrentLapTime");
            var lap = Convert.ToDouble(lapObject);

            int min = (int)(lap / 60);
            double sec = (lap % 60);


            string laptime;

            laptime = string.Format("{0:00}:{1:00.000}", min, sec);

            //laptime = decimalPart > 0
            //    ? string.Format("{0:00}:{1:00}.{2:000}", min, sec, decimalPart.ToString().Substring(2, decimalPartLength))
            //    : laptime = string.Format("{0:00}:{1:00}.{2:000}", 0, 0, 0);

            sessionForm.Laptime_value.Text = laptime;
        }


        public void CarSettingPanelFlash<T>(SdkWrapper.TelemetryUpdatedEventArgs e, ref T actualValue, ref T valueTemp,
            string title)
        {
            #region Set Traction Control TC1, TC2, Boost

            try
            {
                if (!actualValue.Equals(valueTemp))
                {
                    if (sessionDash.settingLabelTitle.Visible == false)
                        settingPanelFpsCounter = 0;

                    //panelek megjelenitése
                    if (settingPanelFpsCounter <= (int)(sessionWrapper.TelemetryUpdateFrequency / settingPanelFps))
                    {
                        sessionDash.settingLabelTitle.Text = title;
                        if (actualValue.GetType() == typeof(float))
                        {
                            sessionDash.settingLabelValue.Text = string.Format("{0:0.00}", actualValue);
                        }
                        else
                        {
                            sessionDash.settingLabelValue.Text = actualValue.ToString();
                        }


                        //settingsPanel.Visible = true;
                        sessionDash.settingLabelTitle.Visible = true;
                        sessionDash.settingLabelValue.Visible = true;
                    }
                    else
                    {
                        //settingsPanel.Visible = false;
                        sessionDash.settingLabelTitle.Visible = false;
                        sessionDash.settingLabelValue.Visible = false;
                        settingPanelFpsCounter = 0;
                        valueTemp = actualValue;
                    }
                }
            }
            catch (Exception ex)
            {
                sessionDash.settingLabelTitle.Text = title;
                sessionDash.settingLabelValue.Text = "N/A";
            }

            #endregion
        }

        // Or IsNanOrInfinity
        protected bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        protected double GetAverageFromList(List<double> list)
        {
            var max = 0.0d;
            foreach (var value in list)
            {
                max += value;
            }
            return max / list.Count;
        }
    }
}
