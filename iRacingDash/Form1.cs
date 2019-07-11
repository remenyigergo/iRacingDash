using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iRacingSdkWrapper;
using iRSDKSharp;

//EngineWarnings : pits = 16, rpmMaximum = 32, normal = 0
namespace iRacingDash
{
    public partial class Form1 : Form
    {
        private static DateTime now = DateTime.Now;
        private SdkWrapper wrapper;
        private iRacingSDK sdk;

        private static Configurator s = new Configurator();
        public static string logPath = s.Configurate<string>("logPath", "config", "Path");
        public static string dateInString = now.ToString().Replace(' ', '-').Replace('/', '-').Replace(':', '-');
        private Logger newLapLogger;
        private Logger fuelUsageLogger;
        private Logger fuelComputeLogger;

        //LEDS
        private float minRpmPercent;
        private float shiftLight1Percent;
        private float shiftLight2Percent;
        private float redLinePercent;

        //COMMON
        private int DriverCarIdx = -9999;

        private float maxRpm;
        private int lapCount = 0;
        private int lapCountTemp = -1;
        private float lapCountPrevious = 0;
        private float trackLength = -9999;
        private int position;

        private int NonRTCalculationFPS = s.Configurate<int>("fps", "config", "NonRealtimeCalculationsFps");
        private int fpsCounter = 0;


        //*FUEL
        private float maxFuelOfCar = 0;

        private float fuelLevel;

        //**ESTIMATED LAPS
        private double remainingTime;

        private int sessionNumber;
        private int sessionNumberTemp = -1;

        private int subSessionNumber;
        private int subSessionNumberTemp = -1;

        private float fuelLapStart = -1;
        private List<double> fuelUsagePerLap = new List<double>();

        //köridők
        List<double> laptimes = new List<double>();
        float currentLap;


        public Form1()
        {
            InitializeComponent();

            sdk = new iRacingSDK();
            wrapper = new SdkWrapper();

            //CONFIGS
            //Car RPM

            maxRpm = s.Configurate<int>("car", "config", "MaxRpm");

            //Window setup
            var X = s.Configurate<int>("window", "config", "PositionX");
            var Y = s.Configurate<int>("window", "config", "PositionY");
            this.Location = new Point((int)X, (int)Y);

            //ShiftLights setup
            minRpmPercent = s.Configurate<int>("led", "config", "MinimumRPMPercent");
            shiftLight1Percent = s.Configurate<int>("led", "config", "ShiftLightGreenPercent");
            shiftLight2Percent = s.Configurate<int>("led", "config", "ShiftLightYellowPercent");
            redLinePercent = s.Configurate<int>("led", "config", "ShiftLightRedPercent");


            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = s.Configurate<int>("fps", "config", "TelemetryFps");
            wrapper.TelemetryUpdated += OnTelemetryUpdated;
            wrapper.SessionInfoUpdated += OnSessionInfoUpdated;



            led1_1.Visible = false;
            led1_2.Visible = false;
            led1_3.Visible = false;
            led1_4.Visible = false;
            led1_5.Visible = false;
            led1_6.Visible = false;
            led2_1.Visible = false;
            led2_2.Visible = false;
            led2_3.Visible = false;
            led3_1.Visible = false;
            led3_2.Visible = false;
            led3_3.Visible = false;

            Laptime_value.ForeColor = Color.LawnGreen;

        }


        //InPitsV2, BrakeBias, Gear, Fuel, TC
        private void NonRealtimeCalculations(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            
            fpsCounter = 0;
            
            InPitsV2(e);
            CalculateGear(e);
            CalculateFuelUsagePerLap(e);
            CalculateFuel(e);

            #region Brake bias
            var brakebias = wrapper.GetData("dcBrakeBias");
            Brake_bias_value.Text = string.Format("{0:00.00}", Convert.ToDouble(brakebias));
            #endregion

            remainingTime = e.TelemetryInfo.SessionTimeRemain.Value;
            fuelLevel = e.TelemetryInfo.FuelLevel.Value;
            Fuel_remain_value.Text = string.Format("{0:00.00}",fuelLevel);
            //fuelLevel.ToString("##.##");

            if (DriverCarIdx != -9999)
                position = e.TelemetryInfo.CarIdxPosition.Value[DriverCarIdx];

            try
            {
                traction1_value.Text = Convert.ToInt32(wrapper.GetData("dcTractionControl")).ToString();
                traction2_value.Text = Convert.ToInt32(wrapper.GetData("dcTractionControl2")).ToString();
            }
            catch (Exception ex)
            {
                traction1_value.Text = "N/A";
                traction2_value.Text = "N/A";
            }
            
        }


        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            try
            {
                fpsCounter++;

                //Folyamatosan frissülő adatok
                sessionNumber = e.TelemetryInfo.SessionNum.Value;

                if (sessionNumberTemp == -1)
                {
                    //init sessionNum
                    sessionNumberTemp = sessionNumber;
                }


                if (sessionNumberTemp != sessionNumber || subSessionNumber != subSessionNumberTemp)
                {
                    //resetting variables
                    lapCount = 0;
                    lapCountTemp = -1;
                    lapCountPrevious = 0;

                    fuelLapStart = -1;
                    fuelUsagePerLap = new List<double>();
                    DriverCarIdx = -9999;
                    remainingTime = 0;
                    fuelLevel = 0;
                    trackLength = -9999;


                    Fuel_to_fill_value.Text = "N/A";
                    Fuel_remain_value.Text = "N/A";
                    Laps_estimate_value.Text = "N/A";
                    Brake_bias_value.Text = "N/A";
                    Speed_value.Text = "N/A";
                    Delta_value.Text = "N/A";
                    traction1_value.Text = "N/A";
                    traction2_value.Text = "N/A";
                    Laptime_value.Text = "00:00.000";
                    rpm.Text = "N/A";
                    gear.Text = "N";
                    Last_lap_value.Text = "N/A";

                    sessionNumberTemp = sessionNumber;

                    //newLapLogger.Log("Resetting variables", Environment.NewLine + "OnTelemetryUpdated");
                }
                else
                {
                    //újraszámoljuk a szükséges adatokat
                    CalculateSpeed(e);
                    UpdateRpmLights(e);
                    NewLapCalculation(e);


                    if (fpsCounter == wrapper.TelemetryUpdateFrequency / NonRTCalculationFPS)
                        NonRealtimeCalculations(e);

                    //az UpdateLapTime mögé kellett rakjam, hogy legyen egy temp kör szám, így az updatelaptimeban majd a legfrissebbel hasonlitja ezt ami előtte bennevolt
                    lapCountTemp = e.TelemetryInfo.Lap.Value;


                    //Textek kiirása
                    rpm.Text = Math.Round(e.TelemetryInfo.RPM.Value).ToString();

                }

            }
            catch (Exception ex)
            {
                Logger errorLogger = new Logger(iRacingDash.Form1.logPath + iRacingDash.Form1.dateInString + "\\errorLog.txt");
                errorLogger.Log("OnSessionTelemetryUpdated Error", ex.Message);
            }
        }

        private void InPitsV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var engineWarning = wrapper.GetData("EngineWarnings");
            var limiter = Convert.ToInt32(engineWarning);

            var onPitRoad = Convert.ToBoolean(wrapper.GetData("OnPitRoad"));

            if (limiter == 16 || limiter == 48) //16 = pit limiter
            {
                if (onPitRoad)
                {
                    Pit_Limiter_title.Visible = true;
                }
                else
                {
                    Pit_Limiter_title.Visible = false;
                }

                speed_limiter.Visible = true;
                Pit_limiter_background_panel.Visible = true;
            }
            else
            {
                if (onPitRoad)
                {
                    Pit_Limiter_title.Visible = true;
                }
                else
                {
                    Pit_Limiter_title.Visible = false;
                }

                Pit_Limiter_title.Visible = false;
                Pit_limiter_background_panel.Visible = false;
            }


        }

        private void CalculateGear(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var gearValue = e.TelemetryInfo.Gear.Value;
            if (gearValue == -1)
                gear.Text = "R";
            else if (gearValue == 0)
                gear.Text = "N";
            else gear.Text = gearValue.ToString();
        }

        private void CalculateSpeed(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            float speedInMs = e.TelemetryInfo.Speed.Value;
            double speedInKmh = speedInMs * 3.6;
            Speed_value.Text = Math.Round(speedInKmh).ToString();
        }

        private void NewLapCalculation(SdkWrapper.TelemetryUpdatedEventArgs e)
        {

            NewLapSettings(e);
            UpdateLapTimeV2(e);
            CalculateDeltaV2(e);
        }

        private void NewLapSettings(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapDist = e.TelemetryInfo.LapDist.Value;
            lapCount = e.TelemetryInfo.Lap.Value;

            //init lapcountTemp
            if (lapCountTemp == -1)
            {
                lapCountTemp = lapCount;
                currentLap = lapCount;

                //creating the logger here
                //fuelComputeLogger = new Logger(logPath + dateInString + "\\fuelCompute.txt"); TODO
                newLapLogger = new Logger(logPath + dateInString + "\\newLap.txt");
                this.newLapLogger.Log("Init", "lapCountTemp: " + lapCountTemp + Environment.NewLine + "  currentLap: " + currentLap);
            }

            if (lapCount - lapCountTemp == 1)
            {
                var lapTime = wrapper.GetData("LapLastLapTime");
                var lapTimeConverted = Convert.ToDouble(lapTime);
                if (lapTimeConverted != 0 && lapTimeConverted != -1)
                {
                    laptimes.Add(lapTimeConverted);
                }

                //laptimes.Add(lapTime);
                lapCountPrevious = currentLap;
                currentLap = lapCount;

                newLapLogger.Log("In a new lap", "lapTimeConverted: " + lapTimeConverted + Environment.NewLine + "  lapCountPrevious: " + lapCountPrevious + Environment.NewLine + "  currentLap: " + currentLap);
                
            }
        }

        private void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var deltaObject = wrapper.GetData("LapDeltaToSessionBestLap");
            var deltaInt = Convert.ToDouble(deltaObject);

            Delta_value.Text = string.Format("{0:0.00}", deltaInt); deltaInt.ToString("+0" + ".##");

            //if (deltaInt > 0)
            //{
            //    if (deltaInt < 1)
            //    {
            //        Delta_value.Text = string.Format("{0:0.00}",deltaInt); deltaInt.ToString("+0" + ".##");
            //    }
            //    else
            //    {
            //        Delta_value.Text = deltaInt.ToString("+##.##");
            //    }

            //}
            //else

            //if (deltaInt < 0)
            //{
            //    if (deltaInt > -1)
            //    {
            //        Delta_value.Text = deltaInt.ToString("0" + ".##");
            //    }
            //    else
            //    {
            //        Delta_value.Text = deltaInt.ToString("##.##");
            //    }

            //}
            //else

            //if (deltaInt == 0)
            //{
            //    Delta_value.Text = deltaInt.ToString("0.00");
            //}
            //else
            //if (deltaInt % 1 == 0)
            //{
            //    Delta_value.Text = deltaInt.ToString("0.00");
            //}

        }

        private void CalculateFuelUsagePerLap(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            if (lapCount - lapCountTemp == 1)
            {
                //új kör esetén
                var actualFuelLevel = e.TelemetryInfo.FuelLevel.Value;

                if (fuelLapStart != -1 && fuelLapStart - actualFuelLevel > 0)
                {
                    fuelUsagePerLap.Add(fuelLapStart - actualFuelLevel);
                }



                //fuelLapStart init
                if (fuelLapStart == -1)
                {
                    fuelLapStart = actualFuelLevel;

                    fuelUsageLogger = new Logger(logPath + dateInString + "\\fuelUsage.txt");
                    fuelUsageLogger.Log("Init", "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
                    return;
                }


                var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;
                fuelUsageLogger.Log("Difference", "Lap number: " + lapCount + Environment.NewLine + "  lapStartEndFuelDifference: " + lapStartEndFuelDifference + Environment.NewLine
                    + "  fuelLapStart: " + fuelLapStart + Environment.NewLine + "  actualFuelLevel: " + actualFuelLevel);

                if (lapStartEndFuelDifference == 0)
                {
                    Last_lap_value.Text = "N/A";
                }
                else
                {
                    if (lapStartEndFuelDifference < 1 && lapStartEndFuelDifference > 0)
                    {
                        Last_lap_value.Text = lapStartEndFuelDifference.ToString("0.##");
                    }
                    else if (lapStartEndFuelDifference > 1)
                    {
                        Last_lap_value.Text = lapStartEndFuelDifference.ToString("#.##");
                    }

                }

                fuelLapStart = actualFuelLevel;


            }
        }

        private void UpdateLapTimeV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapObject = wrapper.GetData("LapCurrentLapTime");
            var lap = Convert.ToDouble(lapObject);

            int min = (int)(lap / 60);
            int sec = (int)(lap % 60);


            double decimalPart = lap - Math.Truncate(lap);
            string laptime;
            if (decimalPart > 0)
            {
                laptime = string.Format("{0:00}:{1:00}.{2:000}",min,sec,decimalPart.ToString().Substring(2, 3));
                //laptime = min.ToString("00") + ":" + sec.ToString("00") + "." + decimalPart.ToString().Substring(2, 3);
            }
            else
            {
                laptime = string.Format("{0:00}:{1:00}.{2:000}", 0, 0, 0);
            }

            Laptime_value.Text = laptime;
        }

        private void UpdateRpmLights(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var rpm = e.TelemetryInfo.RPM.Value;
            //var maxRpm = e.TelemetryInfo.CarIdxRPM.Value;

            var rpmPercent = rpm / (maxRpm / 100);

            //ha zöld előtt járunk minden led off
            if (rpmPercent < minRpmPercent)
            {
                //zöldek
                led1_1.Visible = false;
                led1_2.Visible = false;
                led1_3.Visible = false;
                led1_4.Visible = false;
                led1_5.Visible = false;
                led1_6.Visible = false;

                //sárga
                led2_1.Visible = false;
                led2_2.Visible = false;
                led2_3.Visible = false;

                //piros
                led3_1.Visible = false;
                led3_2.Visible = false;
                led3_3.Visible = false;
            }

            //ha a zöldben járunk
            if (rpmPercent >= minRpmPercent && rpmPercent < minRpmPercent + shiftLight1Percent)
            {
                //alsó korlát
                var rpmGreenBottom = maxRpm / 100 * minRpmPercent;
                //felső korlát
                var rpmGreenTop = maxRpm / 100 * (minRpmPercent + shiftLight1Percent);

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmGreenTop - rpmGreenBottom;

                //felosztás
                var oneSixth = diff / 6;

                //
                var percent1 = (rpmGreenBottom + oneSixth) / (maxRpm / 100);
                var percent2 = (rpmGreenBottom + oneSixth * 2) / (maxRpm / 100);
                var percent3 = (rpmGreenBottom + oneSixth * 3) / (maxRpm / 100);
                var percent4 = (rpmGreenBottom + oneSixth * 4) / (maxRpm / 100);
                var percent5 = (rpmGreenBottom + oneSixth * 5) / (maxRpm / 100);
                var percent6 = (rpmGreenBottom + oneSixth * 6) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= minRpmPercent)
                {
                    led1_1.Visible = true;
                    led1_2.Visible = false;
                    led1_3.Visible = false;
                    led1_4.Visible = false;
                    led1_5.Visible = false;
                    led1_6.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    led1_1.Visible = true;
                    led1_2.Visible = true;
                    led1_3.Visible = false;
                    led1_4.Visible = false;
                    led1_5.Visible = false;
                    led1_6.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    led1_1.Visible = true;
                    led1_2.Visible = true;
                    led1_3.Visible = true;
                    led1_4.Visible = false;
                    led1_5.Visible = false;
                    led1_6.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    // 4   
                    led1_1.Visible = true;
                    led1_2.Visible = true;
                    led1_3.Visible = true;
                    led1_4.Visible = true;
                    led1_5.Visible = false;
                    led1_6.Visible = false;
                }
                else if (rpmPercent > percent4 && rpmPercent < percent5)
                {
                    // 5
                    led1_1.Visible = true;
                    led1_2.Visible = true;
                    led1_3.Visible = true;
                    led1_4.Visible = true;
                    led1_5.Visible = true;
                    led1_6.Visible = false;
                }
                else if (rpmPercent > percent5 && rpmPercent < minRpmPercent + shiftLight1Percent)
                {
                    // 6
                    led1_1.Visible = true;
                    led1_2.Visible = true;
                    led1_3.Visible = true;
                    led1_4.Visible = true;
                    led1_5.Visible = true;
                    led1_6.Visible = true;
                }

                led2_1.Visible = false;
                led2_2.Visible = false;
                led2_3.Visible = false;

                led3_1.Visible = false;
                led3_2.Visible = false;
                led3_3.Visible = false;


                //gear changes to white
                gear.ForeColor = Color.Black;

                gear_panel.BackColor = Color.Gold;
                gear.ForeColor = Color.Black;
            }



            if (rpmPercent > minRpmPercent + shiftLight1Percent &&
                rpmPercent <= minRpmPercent + shiftLight1Percent + shiftLight2Percent)
            {
                //ha a sárgában vagyunk

                //alapból az összest kivilágitjuk a zöldekből
                led1_1.Visible = true;
                led1_2.Visible = true;
                led1_3.Visible = true;
                led1_4.Visible = true;
                led1_5.Visible = true;
                led1_6.Visible = true;


                //alsó korlát
                var rpmYellowBottom = maxRpm / 100 * (minRpmPercent + shiftLight1Percent);
                //felső korlát
                var rpmYellowTop = maxRpm / 100 * (minRpmPercent + shiftLight1Percent + shiftLight2Percent);

                //megmondom mennyi az rpm különbség amit 3 részre osztok
                var diff = rpmYellowTop - rpmYellowBottom;

                //felosztás
                var oneThird = diff / 3;

                var percent1 = (rpmYellowBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmYellowBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmYellowBottom + oneThird * 3) / (maxRpm / 100);



                if (rpmPercent > minRpmPercent + shiftLight1Percent && rpmPercent < percent1)
                {
                    //1
                    led2_1.Visible = true;
                    led2_2.Visible = false;
                    led2_3.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    led2_1.Visible = true;
                    led2_2.Visible = true;
                    led2_3.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    led2_1.Visible = true;
                    led2_2.Visible = true;
                    led2_3.Visible = true;
                }

                led3_1.Visible = false;
                led3_2.Visible = false;
                led3_3.Visible = false;

                //gear changes to white
                gear.ForeColor = Color.White;

                gear_panel.BackColor = Color.Gold;
                gear.ForeColor = Color.Black;

            }


            if (rpmPercent > redLinePercent)
            {
                //ha a pirosban vagyunk

                //alapból az összest kivilágitjuk zöldből és sárgából
                led1_1.Visible = true;
                led1_2.Visible = true;
                led1_3.Visible = true;
                led1_4.Visible = true;
                led1_5.Visible = true;
                led1_6.Visible = true;
                led2_1.Visible = true;
                led2_2.Visible = true;
                led2_3.Visible = true;

                //alsó korlát
                var rpmRedBottom = maxRpm / 100 * (minRpmPercent + shiftLight1Percent + shiftLight2Percent);
                //felső korlát
                var rpmRedTop = maxRpm;

                //megmondom mennyi az rpm különbség amit 3 részre osztok
                var diff = rpmRedTop - rpmRedBottom;

                //felosztás
                var oneThird = diff / 3;

                var percent1 = (rpmRedBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmRedBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmRedBottom + oneThird * 3) / (maxRpm / 100);

                if (rpmPercent > minRpmPercent + shiftLight1Percent + shiftLight2Percent && rpmPercent < percent1)
                {
                    //1
                    led3_1.Visible = true;
                    led3_2.Visible = false;
                    led3_3.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    led3_1.Visible = true;
                    led3_2.Visible = true;
                    led3_3.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    led3_1.Visible = true;
                    led3_2.Visible = true;
                    led3_3.Visible = true;
                }

                //gear changes to red
                gear.ForeColor = Color.Red;
                gear_panel.BackColor = Color.Orange;



            }

            if (rpmPercent > redLinePercent)
            {
                //ha már villogni kell
                //TODO

                gear_panel.BackColor = Color.Red;
                gear.ForeColor = Color.White;

            }
        }

        private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            try
            {


                subSessionNumber = Int32.Parse(e.SessionInfo["WeekendInfo"]["SubSessionID"].Value);

                if (subSessionNumberTemp == -1)
                {
                    //init SubSessionIdTemp
                    subSessionNumberTemp = subSessionNumber;
                }

                if (sessionNumber != sessionNumberTemp || subSessionNumber != subSessionNumberTemp)
                {
                    //reset
                    maxFuelOfCar = -9999;
                    trackLength = 0;
                    subSessionNumberTemp = subSessionNumber;

                    lapCount = 0;
                    lapCountTemp = -1;
                    lapCountPrevious = 0;

                    fuelLapStart = -1;
                    fuelUsagePerLap.Clear();
                    DriverCarIdx = -9999;
                    remainingTime = 0;
                    fuelLevel = 0;
                    trackLength = -9999;

                    Fuel_to_fill_value.Text = "N/A";
                    Fuel_remain_value.Text = "N/A";
                    Laps_estimate_value.Text = "N/A";
                    Brake_bias_value.Text = "N/A";
                    Speed_value.Text = "N/A";
                    Delta_value.Text = "N/A";
                    traction1_value.Text = "N/A";
                    traction2_value.Text = "N/A";
                    Laptime_value.Text = "00:00.000";
                    rpm.Text = "N/A";
                    gear.Text = "N";
                    Last_lap_value.Text = "N/A";

                    //newLapLogger.Log("Resetting variables", Environment.NewLine + "OnSessionUpdated");
                }
                else
                {
                    maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
                    trackLength = float.Parse(e.SessionInfo["WeekendInfo"]["TrackLength"].Value.Substring(0, 4)) * 1000;
                }
            }
            catch (Exception ex)
            {
                
            }
        }


        private void CalculateFuel(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //required vars
            //remaining time, avg fuel usage, avg laptime
            var avgFuelUsage = GetAverageFromList(fuelUsagePerLap);
            var avgLapTime = GetAverageFromList(laptimes);

            var remainingLapsWithFuel = fuelLevel / avgFuelUsage;
            if (HasValue(remainingLapsWithFuel))
                Laps_estimate_value.Text = string.Format("{0:00.00}",remainingLapsWithFuel);
                //remainingLapsWithFuel.ToString("##.##");

            //to fill to the end
            var timeWithoutFuel = remainingTime - (avgLapTime * remainingLapsWithFuel);

            if (timeWithoutFuel > 0)
            {
                var floorLapsToFill = Math.Floor(timeWithoutFuel / avgLapTime);
                var leftOverSeconds = timeWithoutFuel % avgLapTime;

                //calculate fuel for leftover seconds
                var fillForLeftover = (leftOverSeconds / (avgLapTime / 100)) * (avgFuelUsage / 100);


                if (HasValue(floorLapsToFill) && HasValue(fillForLeftover))
                {
                    var toFill = (floorLapsToFill * avgFuelUsage) + fillForLeftover + Math.Ceiling(avgFuelUsage);
                    if (toFill > maxFuelOfCar)
                    {
                        Fuel_to_fill_value.Text = string.Format("{0:00.00}",maxFuelOfCar);
                        //maxFuelOfCar.ToString("##.##");
                    }
                    else
                    {
                        Fuel_to_fill_value.Text = string.Format("0:00.00", toFill);
                        //toFill.ToString("##.##");
                    }
                }
            }
            else
            {
                Fuel_to_fill_value.Text = "ENGH";
            }


            if (fuelLevel < 10)
            {
                panel4.BackColor = Color.DarkRed;
            }
            else
            {
                panel4.BackColor = Color.FromArgb(0, 40, 40, 40);
            }
        }

        // Or IsNanOrInfinity
        public bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        private double GetAverageFromList(List<double> list)
        {
            var max = 0.0d;
            foreach (var value in list)
            {
                max += value;
            }
            return max / list.Count;
        }




        #region FORM BETÖLTÉS

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private bool clicked = false;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!clicked)
                clicked = true;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
                this.Location = new Point(Location.X + e.X, Location.Y + e.Y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (clicked)
                clicked = false;
        }

        #endregion
    }
}