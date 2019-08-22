using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRacingSdkWrapper;
using iRacingSdkWrapper.Bitfields;

namespace iRacingDash.Sessions
{
    public class OfflineTestingSession : Session
    {
        public OfflineTestingSession(int nonRtFps, Form1 form, SdkWrapper sessionWrapper, Dash sessionDash) : base(nonRtFps, form, sessionWrapper, sessionDash)
        {
        }

        public override void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            try
            {
                fpsCounter++;
                flashingFpsCounter++;
                settingPanelFpsCounter++;

                //Folyamatosan frissülő adatok
                sessionNumber = e.TelemetryInfo.SessionNum.Value;

                sessionForm.Speed_value.Text = Math.Round(e.TelemetryInfo.Speed.Value * 3.6).ToString();

                //újraszámoljuk a szükséges adatokat
                sessionDash.UpdateRpmLights(e);
                sessionDash.CarLeftRight(e);
                sessionDash.UpdateLapTimeV2(e);
                sessionDash.CalculateDeltaV2(e);

                if (fpsCounter == (int)(sessionWrapper.TelemetryUpdateFrequency / NonRTCalculationFPS))
                    NonRealtimeCalculations(e);

                if (flashingFpsCounter == (int)(sessionWrapper.TelemetryUpdateFrequency / flashingFps))
                    WarningFlashes(e);

                #region Set boost (if exist)
                if (sessionWrapper.GetData("dcBoostLevel") != null)
                {
                    boost = Int32.Parse(sessionWrapper.GetData("dcBoostLevel").ToString());
                    sessionForm.boost_value.Text = boost.ToString();
                }
                #endregion

                #region Set TC1 TC2
                if (sessionWrapper.GetData("dcTractionControl") != null)
                    tractionControl1 = Int32.Parse(sessionWrapper.GetData("dcTractionControl").ToString());

                if (sessionWrapper.GetData("dcTractionControl2") != null)
                    tractionControl2 = Int32.Parse(sessionWrapper.GetData("dcTractionControl2").ToString());
                #endregion

                #region Set Brake bias
                if (sessionWrapper.GetData("dcBrakeBias") != null)
                {
                    sessionForm.Brake_bias_value.Text = string.Format("{0:00.00}", Convert.ToDouble(sessionWrapper.GetData("dcBrakeBias").ToString()));
                    brakeBias = float.Parse(sessionWrapper.GetData("dcBrakeBias").ToString());
                }
                #endregion

                #region Set Engine Map
                if (sessionWrapper.GetData("dcThrottleShape") != null)
                {
                    engineMap = Int32.Parse(sessionWrapper.GetData("dcThrottleShape").ToString());
                    sessionForm.boost_value.Text = engineMap.ToString();
                }
                #endregion

                #region Init temp values for car settings
                if (boostTemp == -1 && boost != 0)
                    boostTemp = boost;

                if (tractionControl1Temp == -1 && tractionControl1 != 0)
                    tractionControl1Temp = tractionControl1;

                if (tractionControl2Temp == -1 && tractionControl2 != 0)
                    tractionControl2Temp = tractionControl2;

                if (brakeBiasTemp == -1 && brakeBias != 0)
                    brakeBiasTemp = brakeBias;

                if (engineMapTemp == -1 && engineMap != 0)
                    engineMapTemp = engineMap;
                #endregion

                #region Any setting on car flashes here
                CarSettingPanelFlash(e, ref brakeBias, ref brakeBiasTemp, "FRT BRB");
                CarSettingPanelFlash(e, ref boost, ref boostTemp, "BOOST");
                CarSettingPanelFlash(e, ref tractionControl1, ref tractionControl1Temp, "TC1");
                CarSettingPanelFlash(e, ref tractionControl2, ref tractionControl2Temp, "TC2");

                CarSettingPanelFlash(e, ref engineMap, ref engineMapTemp, "STRAT");
                #endregion

                lapCountTemp = e.TelemetryInfo.Lap.Value;

                sessionForm.rpm.Text = Math.Round(e.TelemetryInfo.RPM.Value).ToString();
            }
            catch (Exception ex)
            {
                if (errorLogger == null)
                    errorLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\errorLog.txt");
                errorLogger.Log("OnSessionTelemetryUpdated Error", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void NewLapSettings(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            lapDist = e.TelemetryInfo.LapDist.Value;
            lapCount = e.TelemetryInfo.Lap.Value;

            #region Init lapCountTemp
            if (lapCountTemp == -1)
            {
                lapCountTemp = lapCount;
                currentLap = lapCount;

                //creating the logger here
                //fuelComputeLogger = new Logger(logPath + dateInString + "\\fuelCompute.txt"); TODO
                newLapLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\newLap.txt");
                this.newLapLogger.Log("Init",
                    "lapCountTemp: " + lapCountTemp + Environment.NewLine + "  currentLap: " + currentLap);
            }
            #endregion

            if (lapCount - lapCountTemp == 1)
            {
                //this is for the first launch, to not store biiig laptimes
                if (initLapTime)
                {
                    initLapTime = false;
                }
                else
                {
                    new Thread(() =>
                    {
                        Thread.Sleep(2000);
                        lapTimeString = sessionWrapper.GetData("LapLastLapTime").ToString();
                        lapTime = Convert.ToDouble(lapTimeString);

                        lapCountPrevious = currentLap;
                        currentLap = lapCount;

                        newLapLogger.Log("In a new lap",
                            "lapTime: " + lapTime +
                            Environment.NewLine + "  lapCountPrevious: " + lapCountPrevious +
                            Environment.NewLine + "  currentLap: " + currentLap +
                            Environment.NewLine + "  initLapTime: " + initLapTime);
                    }).Start();
                }
            }
        }

        public override void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            try
            {
                maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
                trackLength = float.Parse(e.SessionInfo["WeekendInfo"]["TrackLength"].Value.Substring(0, 4)) * 1000;

                var isMaxRpmValid = float.TryParse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value,
                    out sessionDash.maxRpm);

                if (isMaxRpmValid)
                    sessionDash.maxRpm = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value);

                sessionType = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["SessionType"].Value;
                raceWeek = e.SessionInfo["WeekendInfo"]["RaceWeek"].Value;
                eventType = e.SessionInfo["WeekendInfo"]["EventType"].Value;
                eventType = eventType.Replace(' ', '_');
                trackName = e.SessionInfo["WeekendInfo"]["TrackName"].Value;
                trackName = trackName.Replace(' ', '_');
            }
            catch (Exception ex)
            {
                if (errorLogger == null)
                    errorLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\errorLog.txt");
                errorLogger.Log("OnSessionInfoUpdated Error", ex.Message);
            }
        }

        protected override void NonRealtimeCalculations(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            fpsCounter = 0;

            InPitsV2(e);
            CalculateGear(e);
            CalculateFuelUsagePerLap(e);

            remainingTime = e.TelemetryInfo.SessionTimeRemain.Value;

            #region Set Fuel level
            fuelLevel = e.TelemetryInfo.FuelLevel.Value;
            sessionForm.Fuel_remain_value.Text = string.Format("{0:00.00}", fuelLevel);
            #endregion

            #region EngineWarnings (Stalled engine)
            sessionForm.engine_panel.Visible = engineWarning == 30 || engineWarning == 14 ? true : false;
            #endregion


            if (DriverCarIdx != -9999)
                position = e.TelemetryInfo.CarIdxPosition.Value[DriverCarIdx];
        }

        private void CalculateFuelUsagePerLap(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            if (isItNewLap())
            {
                var actualFuelLevel = e.TelemetryInfo.FuelLevel.Value;

                InitFuelOnLapStart(actualFuelLevel);
                DisplayLastLapUsage(actualFuelLevel);
            }
        }

        private bool isItNewLap()
        {
            return lapCount - lapCountTemp == 1;
        }

        private void InitFuelOnLapStart(float actualFuelLevel)
        {
            if (fuelLapStart == -1)
            {
                fuelLapStart = actualFuelLevel;

                fuelUsageLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\fuelUsage.txt");
                fuelUsageLogger.Log("Init",
                    "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
                return;
            }
        }

        private void DisplayLastLapUsage(float actualFuelLevel)
        {
            if (fuelUsageLogger == null)
            {
                fuelUsageLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\fuelUsage.txt");
                fuelUsageLogger.Log("Init",
                    "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
            }

            var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;

            if (lapStartEndFuelDifference > 0)
            {
                sessionForm.Last_lap_value.Text = string.Format("{0:0.00}", lapStartEndFuelDifference);
                sessionForm.Last_lap_value.ForeColor = Color.FromArgb(255, 128, 0);
            }
            else
            {
                sessionForm.Last_lap_value.ForeColor = Color.Yellow;
            }

            
            

            fuelUsageLogger.Log("Difference", "Lap number: " + lapCount +
                                              Environment.NewLine + "  fuelLapStart: " + fuelLapStart +
                                              Environment.NewLine + "  actualFuelLevel: " + actualFuelLevel +
                                              Environment.NewLine + "  lapStartEndFuelDifference: " +
                                              lapStartEndFuelDifference +
                                              Environment.NewLine + "  fuelLapStart is set to actualFuelLevel");

            fuelLapStart = actualFuelLevel;
        }

        protected override void FlashFlags(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var sessionFlag = e.TelemetryInfo.SessionFlags.Value?.ToString();
            if (sessionFlag == null) return;

            var sessionFlags = (SessionFlags)Enum.Parse(typeof(SessionFlags), sessionFlag.Replace('|', ','));

            switch (sessionFlags)
            {
                case var t when t.HasFlag(SessionFlags.Repair):
                    LightPanel(sessionForm.warning_panel, Color.Black);
                    break;
                case var t when t.HasFlag(SessionFlags.Blue):
                    LightPanel(sessionForm.warning_panel, Color.Blue);
                    break;
                case var t when t.HasFlag(SessionFlags.Yellow):
                case var t1 when t1.HasFlag(SessionFlags.Caution):
                case var t2 when t2.HasFlag(SessionFlags.CautionWaving):
                case var t3 when t3.HasFlag(SessionFlags.YellowWaving):
                    LightPanel(sessionForm.warning_panel, Color.Yellow);
                    break;
                case var t1 when t1.HasFlag(SessionFlags.GreenHeld):
                case var t2 when t2.HasFlag(SessionFlags.OneLapToGreen):
                    LightPanel(sessionForm.warning_panel, Color.Green);
                    break;
                case var t when t.HasFlag(SessionFlags.White):
                    LightPanel(sessionForm.warning_panel, Color.White);
                    break;
                default:
                    sessionForm.warning_panel.BackColor = Color.Transparent;
                    break;
            }
        }
    }
}
