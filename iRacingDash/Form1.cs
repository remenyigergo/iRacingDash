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
        private static string dateInString = now.ToString().Replace(' ', '-').Replace('/', '-').Replace(':', '-');
        private Logger newLapLogger = new Logger(@"C:\Users\Greg\Documents\iRacingDash\logs\"+dateInString+"\\newLap.txt");
        private Logger fuelUsageLogger = new Logger(@"C:\Users\Greg\Documents\iRacingDash\logs\"+dateInString+"\\fuelUsage.txt");
        private Logger fuelComputeLogger = new Logger(@"C:\Users\Greg\Documents\iRacingDash\logs\"+dateInString+"\\fuelUsage.txt");



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
        private float lapCountPrevious = 0;
        private int manualLapCount = 0;
        private float trackLength = -9999;
        private int meterThreshold = 5;
        private bool isItRed = false;

        //TIMER
        private string laptime = "0:00:00";
        Stopwatch lapTimeStopWatch = new Stopwatch();

        //DELTA
        private List<int> distanceOnLapList = new List<int>();
        private List<int> timeDistanceOnLapList = new List<int>();

        Dictionary<int, float> bestLapMeterAndTime = new Dictionary<int, float>();
        Dictionary<int, float> distancesInMeterAndTime = new Dictionary<int, float>();
        private float distanceOnLap = 0;
        private float timeDistanceOnLap = 0;
        private float timeDistanceOnLapTemp = 0;

        //*FUEL
        private float maxFuelOfCar = 0;

        private float fuelLevel;

        //**ESTIMATED LAPS
        private double remainingTime;

        private int sessionNumber;
        private float fuelLapStart = -1;
        private float fuelLapEnd;
        private List<double> fuelUsagePerLap = new List<double>();

        //köridők
        List<double> laptimes = new List<double>();
        float currentLap;
        bool itWasABestLap = false;
        private Dictionary<int, float> bestlap = new Dictionary<int, float>();
        bool lapStarted = false;

        //a sessioninfos fontos
        //DriverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
        //var session = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["ResultsPositions"]["Position", 1]["LastTime"].GetValue(); // ["SessionInfo"]["Sessions"]["SessionNum", 0]["CarIdx", DriverCarIdx];

        public Form1()
        {
            InitializeComponent();

            sdk = new iRacingSDK();
            wrapper = new SdkWrapper();

            //CONFIGS
            //Car RPM
            var s = new Configurator();
            maxRpm = s.Configurate("car", "config", "MaxRpm");

            //Window setup
            var X = s.Configurate("window", "config", "PositionX");
            var Y = s.Configurate("window", "config", "PositionY");
            this.Location = new Point((int)X,(int)Y);

            //ShiftLights setup
            minRpmPercent = s.Configurate("led", "config", "MinimumRPMPercent");
            shiftLight1Percent = s.Configurate("led", "config", "ShiftLightGreenPercent");
            shiftLight2Percent = s.Configurate("led", "config", "ShiftLightYellowPercent");
            redLinePercent = s.Configurate("led", "config", "ShiftLightRedPercent");


            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 30;
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

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //Folyamatosan frissülő adatok
            sessionNumber = e.TelemetryInfo.SessionNum.Value;
            remainingTime = e.TelemetryInfo.SessionTimeRemain.Value;
            fuelLevel = e.TelemetryInfo.FuelLevel.Value;
            distanceFromFinishLine = e.TelemetryInfo.LapDist.Value;
            //var fictionalObject = wrapper.GetTelemetryValue<int>("dcBrakeBias");
            var brakebias = wrapper.GetData("dcBrakeBias");
            Brake_bias_value.Text = Convert.ToDouble(brakebias).ToString("##.##");
            traction1_value.Text = Convert.ToInt32(wrapper.GetData("dcTractionControl")).ToString();
            traction2_value.Text = Convert.ToInt32(wrapper.GetData("dcTractionControl2")).ToString();
            
             
            //var engineWarnings = e.TelemetryInfo //need to test
            //Delta_value.Text = wrapper.GetTelemetryValue<float>("LapDeltaToBestLap").ToString();

            //Automatic_value.Text = e.TelemetryInfo.MGUKDeployFixed.ToString();  //for lmp1

            //újraszámoljuk a szükséges adatokat
            CalculateSpeed(e);
            UpdateRpmLights(e);
            NewLapCalculation(e);
            CalculateGear(e);
            //InPits(e);
            InPitsV2(e);



            //az UpdateLapTime mögé kellett rakjam, hogy legyen egy temp kör szám, így az updatelaptimeban majd a legfrissebbel hasonlitja ezt ami előtte bennevolt
            lapCountTemp = e.TelemetryInfo.Lap.Value;


            //Textek kiirása
            rpm.Text = Math.Round(e.TelemetryInfo.RPM.Value).ToString();
            Fuel_remain_value.Text = fuelLevel.ToString("##.##");

        }

        private void CalculateDelta(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            distanceOnLap = e.TelemetryInfo.LapDist.Value;
            //minden körben tároljuk méterenként azt hol tartunk a pályán, és az időt hozzá
            //azt rakjuk globálba, amelyik a legjobb idő volt és mindig ahhoz nézzük majd a következő köröket.
            if (e.TelemetryInfo.Lap.Value == currentLap && e.TelemetryInfo.Lap.Value - lapCountPrevious == 1 && currentLap > 0) //ha a jelenlegi körben vagyunk
            {
                //ha még nincs beállított session best, akkor az első mért körünket vesszük
                //figyelni kellesz hogy valid lap volt-e. Azaz a CalculateLapTime-ban majd állítani kell egy global változót
                if (bestLapMeterAndTime.Count == 0)
                {
                    var distInMeter = (int)Math.Round(e.TelemetryInfo.LapDist.Value);
                    //label2.Text = distInMeter.ToString();
                    if (distInMeter % meterThreshold == 0 && distInMeter != 0)
                    {
                        var distInMilliseconds = lapTimeStopWatch.ElapsedMilliseconds;
                        if (!distancesInMeterAndTime.ContainsKey(distInMeter))
                        {
                            distancesInMeterAndTime.Add(distInMeter, distInMilliseconds);

                            //distancesInMeterAndTime.Add(distInMeter, distInMilliseconds);
                        }
                    }

                    if (distInMeter > trackLength - meterThreshold && manualLapCount == 1 && trackLength != -9999)
                    {
                        if (bestLapMeterAndTime.Count == 0)
                        {
                            bestLapMeterAndTime = new Dictionary<int, float>(distancesInMeterAndTime);
                            distancesInMeterAndTime.Clear();
                        }
                    }
                }
                else
                {
                    if (bestLapMeterAndTime.Count != 0)
                    {
                        var distInMeter = (int)Math.Round(e.TelemetryInfo.LapDist.Value);
                        if (distInMeter % meterThreshold == 0 && distInMeter != 0 && distInMeter < trackLength - meterThreshold)
                        {
                            var distInMilliseconds = (float)lapTimeStopWatch.ElapsedMilliseconds;

                            //összehasonlitjuk az adott méteren az időkülönbséget
                            var meterAndMillisecondPair = bestLapMeterAndTime.FirstOrDefault(pair => pair.Key == distInMeter);

                            float diff = 0;
                            if (meterAndMillisecondPair.Key > 0 && meterAndMillisecondPair.Value > 0 && distInMeter > 0)
                            {
                                diff = (distInMilliseconds - meterAndMillisecondPair.Value) / (float)1000.0;
                            }
                            else if (meterAndMillisecondPair.Key == 0 && meterAndMillisecondPair.Value == 0 && distInMeter > 0)
                            {
                                //az előtte lévővel iratom ki ha nemtalálunk mért időt adott méterhez
                                bool foundOne = false;
                                int times = 1;
                                while (!foundOne)
                                {
                                    meterAndMillisecondPair = bestLapMeterAndTime.FirstOrDefault(pair => pair.Key == distInMeter - 5 * times);


                                    if (meterAndMillisecondPair.Key != 0 && meterAndMillisecondPair.Value != 0)
                                    {
                                        diff = (distInMilliseconds - meterAndMillisecondPair.Value) / (float)1000.0;
                                        foundOne = true;
                                    }
                                    else
                                    {
                                        times++;
                                    }

                                }
                            }


                            //kiírjuk
                            switch (diff > 0)
                            {
                                case true:
                                    if (diff < 1)
                                    {
                                        Delta_value.Text = diff.ToString("+0.##");
                                    }
                                    else
                                    {
                                        Delta_value.Text = diff.ToString("+##.##");
                                    }

                                    break;
                                case false:
                                    if (diff > -1)
                                    {
                                        Delta_value.Text = diff.ToString("0.##");
                                    }
                                    else
                                    {
                                        Delta_value.Text = diff.ToString("##.##");
                                    }

                                    break;
                            }


                            //tároljuk hogy letudjam copyzni a best lapre
                            if (!distancesInMeterAndTime.ContainsKey(distInMeter))
                            {
                                distancesInMeterAndTime.Add(distInMeter, distInMilliseconds);
                            }


                            //if (itWasABestLap)
                            //{
                            //    //lecopyzzuk
                            //    bestLapMeterAndTime = new Dictionary<int, float>(distancesInMeterAndTime);
                            //    itWasABestLap = false;
                            //}

                        }
                    }

                }

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

            //if (Convert.ToInt32(engineWarning) == 32) 
            //{
            //    //RPM MAX!

            //}
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

            NewLapSettings(e);
            //UpdateLaptime(e);
            UpdateLapTimeV2(e);
            //CalculateDelta(e);
            CalculateDeltaV2(e);
            CalculateFuelUsagePerLap(e);
            CalculateFuel(e);
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

                this.newLapLogger.Log("Init","lapCountTemp: "+lapCountTemp + Environment.NewLine + "  currentLap: "+currentLap);
            }

            if (lapCount - lapCountTemp == 1)
            {
                var lapTime = wrapper.GetData("LapLastLapTime");
                var lapTimeConverted = Convert.ToDouble(lapTime);
                if (lapTimeConverted != 0 && lapTimeConverted!= -1)
                {
                    laptimes.Add(lapTimeConverted);
                }

                //laptimes.Add(lapTime);
                lapCountPrevious = currentLap;
                currentLap = lapCount;

                newLapLogger.Log("In a new lap","lapTimeConverted: " + lapTimeConverted+ Environment.NewLine + "  lapCountPrevious: "+lapCountPrevious+ Environment.NewLine + "  currentLap: "+currentLap);
            }
        }

        private void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var deltaObject = wrapper.GetData("LapDeltaToSessionBestLap");
            var deltaInt = Convert.ToDouble(deltaObject);



            if (deltaInt > 0)
            {
                if (deltaInt < 1)
                {
                    Delta_value.Text = deltaInt.ToString("+0" + ".##");
                }
                else
                {
                    Delta_value.Text = deltaInt.ToString("+##.##");
                }

            } else

            if (deltaInt < 0)
            {
                if (deltaInt > -1)
                {
                    Delta_value.Text = deltaInt.ToString("0" + ".##");
                }
                else
                {
                    Delta_value.Text = deltaInt.ToString("##.##");
                }

            } else

            if (deltaInt == 0)
            {
                Delta_value.Text = deltaInt.ToString("0.00");
            } else
            if (deltaInt % 1 == 0)
            {
                Delta_value.Text = deltaInt.ToString("0.00");
            }
            
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
                    fuelUsageLogger.Log("Init", "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
                    return;
                }
                     

                var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;
                fuelUsageLogger.Log("Difference", "Lap number: " + lapCount + Environment.NewLine + "  lapStartEndFuelDifference: " + lapStartEndFuelDifference + Environment.NewLine 
                    + "  fuelLapStart: "+fuelLapStart + Environment.NewLine + "  actualFuelLevel: "+actualFuelLevel);

                if (lapStartEndFuelDifference == 0)
                {
                    Last_lap_value.Text = "N/A";
                }
                else
                {
                    if (lapStartEndFuelDifference < 1 && lapStartEndFuelDifference > 0)
                    {
                        Last_lap_value.Text = lapStartEndFuelDifference.ToString("0.##");
                    } else if (lapStartEndFuelDifference > 1)
                    {
                        Last_lap_value.Text = lapStartEndFuelDifference.ToString("#.##");
                    }
                    
                }

                fuelLapStart = actualFuelLevel;

                
            }
        }

        private void UpdateLaptime(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapDist = e.TelemetryInfo.LapDist.Value;
            lapCount = e.TelemetryInfo.Lap.Value;

            //init lapcountTemp
            if (lapCountTemp == -1)
            {
                //distancesInMeterAndTime.Clear();
                lapCountTemp = lapCount;
                currentLap = lapCount;
                lapTimeStopWatch.Start();
            }

            if (lapCount - lapCountTemp == 1)
            {
                lapStarted = true;

                lapCountPrevious = currentLap;
                currentLap = lapCount;
                lapTimeStopWatch.Restart();
                manualLapCount++;
            }


            if (lapTimeStopWatch.ElapsedMilliseconds != 0)
            {
                float min = (lapTimeStopWatch.ElapsedMilliseconds / 1000) / 60;
                float sec = (lapTimeStopWatch.ElapsedMilliseconds / 1000) % 60;

                float tenth = (lapTimeStopWatch.ElapsedMilliseconds / 100) % 10;
                float hundredth = (lapTimeStopWatch.ElapsedMilliseconds / 10) % 10;
                float thousandth = lapTimeStopWatch.ElapsedMilliseconds % 10;

                //var s = String.Format("{0}{1}{2}", tenth, hundredth, thousandth);
                string laptime = min + ":" + sec + ":" + tenth + "" + hundredth + "" + thousandth;
                Laptime_value.Text = laptime;
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
                laptime = min.ToString("00") + ":" + sec.ToString("00") + "." + decimalPart.ToString().Substring(2, 3);
            }
            else
            {
                laptime = min.ToString("00") + ":" + sec.ToString("00") + "." + decimalPart.ToString("000");
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
            //StoreLastLapTime(e);
            maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
            trackLength = float.Parse(e.SessionInfo["WeekendInfo"]["TrackLength"].Value.Substring(0, 4)) * 1000;
        }

        private void StoreLastLapTime(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
#pragma Köridők tárolása
            //TODO csak kör befejezése után számitgassunk már
            DriverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
            int i = 1;
            bool theresMoreDriver = true;

            while (theresMoreDriver) //a második feltétel az új kör feltétele, azaz csak új körönként fogom megnézni a köridőt
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

                        //legjobb kör lementéséhez való beállítások
                        if (laptimes[laptimes.Count - 1] > time)
                        {
                            bestLapMeterAndTime = new Dictionary<int, float>(distancesInMeterAndTime);

                            itWasABestLap = true;
                            bestlap.Clear();
                            bestlap.Add(laptimes.Count - 1, time);
                        }
                        else
                        {
                            itWasABestLap = false;
                        }
                    }
                    else if (laptimes.Count == 0)
                    {
                        if (time != -1)
                            laptimes.Add(time);

                        itWasABestLap = true;
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
                    var toFill = (floorLapsToFill * avgFuelUsage) + fillForLeftover + Math.Ceiling(avgFuelUsage);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}