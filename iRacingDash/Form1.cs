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

namespace iRacingDash
{
    public partial class Form1 : Form
    {
        private SdkWrapper wrapper;
        private iRacingSDK sdk;

        //LEDS
        private float minRpmPercent = 50;

        private float shiftLight1Percent = 20;
        private float shiftLight2Percent = 20;
        private float redLinePercent = 90;

        //COMMON
        private int DriverCarIdx = -9999;

        private float maxRpm = 7300; //TODO
        private float distanceFromFinishLine;
        private float distanceFromFinishLine_Previous;
        private int lapCount = 0;
        private int lapCountTemp = -1;

        //TIMER
        private string laptime = "0:00:00";

        Stopwatch lapTimeStopWatch = new Stopwatch();

        //*FUEL
        private float maxFuelOfCar = 0;

        private float fuelLevel;

        //**ESTIMATED LAPS
        private double remainingTime;

        private int sessionNumber;
        private float fuelLapStart = -1;
        private float fuelLapEnd;
        private List<float> fuelUsagePerLap = new List<float>();

        //köridők
        List<float> laptimes = new List<float>();

        float averageLapTime;

        //a sessioninfos fontos
        //DriverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
        //var session = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["ResultsPositions"]["Position", 1]["LastTime"].GetValue(); // ["SessionInfo"]["Sessions"]["SessionNum", 0]["CarIdx", DriverCarIdx];

        public Form1()
        {
            InitializeComponent();

            sdk = new iRacingSDK();
            wrapper = new SdkWrapper();
            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 60;
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

            

            //Close();
        }

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //Folyamatosan frissülő adatok
            sessionNumber = e.TelemetryInfo.SessionNum.Value;
            remainingTime = e.TelemetryInfo.SessionTimeRemain.Value;
            fuelLevel = e.TelemetryInfo.FuelLevel.Value;
            distanceFromFinishLine = e.TelemetryInfo.LapDist.Value;
            
            var sessionBest = wrapper.GetTelemetryValue<float>("LapDeltaToBestLap_OK");
            var active = wrapper.GetTelemetryValue<bool>("IsDiskLoggingActive");
            var enabled = wrapper.GetTelemetryValue<bool>("IsDiskLoggingEnabled");

            try
            {
                var velX = new TelemetryValue<float>(sdk, "LapDeltaToBestLap");
            }
            catch (Exception ex)
            {
                
            }
            
            //var engineWarnings = e.TelemetryInfo
            //Delta_value.Text = wrapper.GetTelemetryValue<float>("LapDeltaToBestLap").ToString();

            //Automatic_value.Text = e.TelemetryInfo.MGUKDeployFixed.ToString();  //for lmp1

            //újraszámoljuk a szükséges adatokat
            CalculateSpeed(e);
            UpdateRpmLights(e);
            NewLapCalculation(e);
            CalculateGear(e);
            InPits(e);

            //az UpdateLapTime mögé kellett rakjam, hogy legyen egy temp kör szám, így az updatelaptimeban majd a legfrissebbel hasonlitja ezt ami előtte bennevolt
            lapCountTemp = e.TelemetryInfo.Lap.Value;

            //Textek kiirása
            rpm.Text = Math.Round(e.TelemetryInfo.RPM.Value).ToString();
            Fuel_remain_value.Text = fuelLevel.ToString("##.##");

        }

        private void InPits(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var onpitroad = e.TelemetryInfo.CarIdxOnPitRoad.Value;
            if (DriverCarIdx != -9999 && onpitroad != null)
            {
                if (onpitroad[DriverCarIdx] == true)
                {
                    Pit_Limiter_title.Visible = true;
                    Pit_limiter_background_panel.Visible = true;
                }
                else
                {
                    Pit_Limiter_title.Visible = false;
                    Pit_limiter_background_panel.Visible = false;
                }
            }
            
        }

        private void CalculateGear(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //correct print
            var gearValue = e.TelemetryInfo.Gear.Value;
            if (gearValue == -1)
                gear.Text = "R";
            else if (gearValue == 0)
                gear.Text = "N";
            else gear.Text = gearValue.ToString();

            

            //gear.Text = e.TelemetryInfo.Gear.ToString();
        }

        private void CalculateSpeed(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            float speedInMs = e.TelemetryInfo.Speed.Value;
            double speedInKmh = speedInMs * 3.6;
            Speed_value.Text = Math.Round(speedInKmh).ToString();
        }

        private void NewLapCalculation(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            UpdateLaptime(e);
            CalculateFuelUsagePerLap(e);
            CalculateFuel(e);
        }

        private void CalculateFuelUsagePerLap(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            if (lapCount - lapCountTemp == 1)
            {
                //új kör esetén
                var actualFuelLevel = e.TelemetryInfo.FuelLevel.Value;

                if (fuelLapStart != -1)
                {
                    fuelUsagePerLap.Add(fuelLapStart - actualFuelLevel);
                }


                //fuelLapStart init
                fuelLapStart = actualFuelLevel;

                var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;
                //fuelLapStart = e.TelemetryInfo.FuelLevel.Value;
            }
        }

        private void UpdateLaptime(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapDist = e.TelemetryInfo.LapDist.Value;
            lapCount = e.TelemetryInfo.Lap.Value;

            //init lapcountTemp
            if (lapCountTemp == -1)
            {
                lapCountTemp = lapCount;
                lapTimeStopWatch.Start();
            }

            if (lapCount - lapCountTemp == 1)
            {
                lapTimeStopWatch.Restart();
            }


            if (lapTimeStopWatch.ElapsedMilliseconds != 0)
            {
                float min = (lapTimeStopWatch.ElapsedMilliseconds / 1000) / 60;
                float sec = (lapTimeStopWatch.ElapsedMilliseconds / 1000) % 60;

                float tenth = (lapTimeStopWatch.ElapsedMilliseconds / 100) % 10;
                float hundredth = (lapTimeStopWatch.ElapsedMilliseconds / 10) % 10;
                float thousandth = lapTimeStopWatch.ElapsedMilliseconds % 10;

                //var s = String.Format("{0}{1}{2}", tenth, hundredth, thousandth);
                string laptime = min + ":" + sec + "." + tenth + "" + hundredth + "" + thousandth;
                Laptime_value.Text = laptime;
            }
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
                gear.ForeColor = Color.White;
            }
            //else
            //{
            //    led1_1.Visible = false;
            //    led1_2.Visible = false;
            //    led1_3.Visible = false;
            //    led1_4.Visible = false;
            //    led1_5.Visible = false;
            //    led1_6.Visible = false;
            //}


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
            }

            if (rpmPercent > redLinePercent)
            {
                //ha már villogni kell
                //TODO
            }
        }

        private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            StoreLastLapTime(e);
            maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
        }

        private void StoreLastLapTime(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
#pragma Köridők tárolása
            //TODO csak kör befejezése után számitgassunk már
            DriverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
            int i = 1;
            bool theresMoreDriver = true;

            while (theresMoreDriver
            ) //a második feltétel az új kör feltétele, azaz csak új körönként fogom megnézni a köridőt
            {
                int carId = -1;
                var carIdInString =
                    e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["ResultsPositions"]
                        ["Position", i]["CarIdx"].GetValue();

                if (carIdInString != null)
                {
                    Int32.TryParse(
                        e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["ResultsPositions"]
                            ["Position", i]["CarIdx"].GetValue(), out carId);
                }
                else
                {
                    //nem tudtuk lekérni a poziciokat, ezért ez azt jelenti, hogy legelsőként vagy a szerveren és nincs beállított mértidő
                    break;
                }


                if (carId == DriverCarIdx)
                {
                    float time = 0.0f;
                    float.TryParse(
                        e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["ResultsPositions"]
                            ["Position", i]["LastTime"].GetValue(), out time);

                    //listába rakom a köridőket
                    if (laptimes.Count != 0 && laptimes[laptimes.Count - 1] != time)
                    {
                        if (time != -1)
                            laptimes.Add(time);
                    }
                    else if (laptimes.Count == 0)
                    {
                        if (time != -1)
                            laptimes.Add(time);
                    }


                    if (time != 0.0f)
                    {
                        //szarmegjelenités csak magamnak
                        //string laptime = (int)time/60+":"+(int)time%60+":"+(time - Math.Truncate(time)).ToString().Substring(2, 3);
                        theresMoreDriver = false;
                    }
                }
                i++;
            }
#pragma
        }


        private void CalculateFuel(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //required vars
            //remaining time, avg fuel usage, avg laptime
            var avgFuelUsage = GetAverageFromList(fuelUsagePerLap);
            var avgLapTime = GetAverageFromList(laptimes);


            var remainingLapsWithFuel = fuelLevel / avgFuelUsage;
            if (HasValue(remainingLapsWithFuel))
                Laps_estimate_value.Text = remainingLapsWithFuel.ToString("##.##");

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
                    var toFill = (floorLapsToFill * avgFuelUsage) + fillForLeftover;
                    if (toFill > maxFuelOfCar)
                    {
                        Fuel_to_fill_value.Text = maxFuelOfCar.ToString("##.##");
                    }
                    else
                    {
                        Fuel_to_fill_value.Text = toFill.ToString("##.##");
                    }
                }
            }
            else
            {
                Fuel_to_fill_value.Text = "0";
            }
        }

        // Or IsNanOrInfinity
        public bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        private float GetAverageFromList(List<float> list)
        {
            var max = 0.0f;
            foreach (var value in list)
            {
                max += value;
            }
            return max / list.Count;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(1200, 600);
        }
    }
}