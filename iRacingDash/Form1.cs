using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using iRacingSdkWrapper;
using iRSDKSharp;

//EngineWarnings : pits = 16, rpmMaximum = 32, normal = 0, stalled = 30
namespace iRacingDash
{
    public partial class Form1 : Form
    {
        private static DateTime now = DateTime.Now;
        private SdkWrapper wrapper;
        private iRacingSDK sdk;


        private delegate void SafeCallDelegate(bool visible, string time);
        private static Configurator s = new Configurator();
        public static string logPath = s.Configurate<string>("logPath", "config", "Path");
        public static string dateInString = now.ToString().Replace(' ', '-').Replace('/', '-').Replace(':', '-');
        private Logger newLapLogger;
        private Logger fuelUsageLogger;
        private Logger fuelComputeLogger;
        private Logger errorLogger;

        //LEDS
        private float minRpmPercent;

        private float shiftLight1Percent;
        private float shiftLight2Percent;
        private float redLinePercent;

        //COMMON
        private int DriverCarIdx = -9999;

        private int gearValue = 0;

        private float maxRpm;
        private float lapDist = 0;
        private int lapCount = 0;
        private int lapCountTemp = -1;
        private float lapCountPrevious = 0;
        private float trackLength = -9999;
        private int position;
        private bool initForFuel = true;
        private int engineWarning;

        private int NonRTCalculationFPS = s.Configurate<int>("fps", "config", "NonRealtimeCalculationsFps");
        private int fpsCounter = 0;
        private int flashingFps = 2;
        private int flashingFpsCounter = 0;

        private int settingPanelFps = 2;
        private int settingPanelFpsCounter = -1;


        //*FUEL
        private float maxFuelOfCar = 0;

        private float fuelLevel;
        private int manualLapCount = -1;

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

        //Delta
        private int deltaLimit = 2;

        //Panelek
        private Panel settingsPanel;

        private Label settingLabelTitle;
        private Label settingLabelValue;

        private Label idleClock;


        //Settings on car
        private int boost;

        private int boostTemp = -1;

        private int tractionControl1;
        private int tractionControl1Temp = -1;

        private int tractionControl2;
        private int tractionControl2Temp = -1;

        private float brakeBias;
        private float brakeBiasTemp = -1;

        //
        private PictureBox idleImg = new PictureBox();

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

            //Create labels and panels
            settingLabelTitle = CreateLabel("settingLabelTitle", "Setting", new Size(this.Width, 150), new Point(0, 0),
                Color.Black, Color.DarkGray, new Font("Microsoft YaHei", 40, FontStyle.Bold), false);
            settingLabelValue = CreateLabel("settingLabelValue", "Value", new Size(this.Width, 150), new Point(0, 150),
                Color.Black, Color.DarkGray, new Font("Microsoft YaHei", 40, FontStyle.Bold), false);
            settingLabelTitle.TextAlign = ContentAlignment.BottomCenter;
            settingLabelValue.TextAlign = ContentAlignment.TopCenter;

            settingsPanel = CreateSettingWindow(new Point(0, 0), Color.DarkGray, new Size(this.Width, this.Height),
                false);


            CreateIdle();


            //panelek előtérbe helyezése
            settingLabelTitle.BringToFront();
            settingLabelValue.BringToFront();
            //settingsPanel.BringToFront();


            Thread t = new Thread(new ThreadStart(CheckWrapperRunning));
            t.Start();
        }

        private void WriteTextSafe(bool visible, string time)
        {
            if (idleImg.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                Invoke(d, new object[] { visible, time });
            }
            else
            {
                idleImg.Visible = visible;
                idleClock.Text = time;
            }

            if (idleClock.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                Invoke(d, new object[] { visible, time });
            }
            else
            {
                idleClock.Visible = visible;
                idleClock.Text = time;
            }

        }


        private void SetText(bool value, string time)
        {
            WriteTextSafe(value, time);
        }

        private void CheckWrapperRunning()
        {
            idleClock.Text = string.Format("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute);
            while (true)
            {
                var isConnected = wrapper.IsConnected;

                if (isConnected)
                {
                    WriteTextSafe(false, string.Format("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
                }
                else
                {
                    WriteTextSafe(true, string.Format("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
                }


                Thread.Sleep(3000);
            }

        }

        private void CreateIdle()
        {
            Image image = iRacingDash.Properties.Resources.idle;
            this.idleImg = new PictureBox
            {
                Name = "idleImg",
                Size = new Size(370, 300),
                Location = new Point(0, 0),
                Image = image,
            };
            this.Controls.Add(idleImg);
            idleImg.MouseDown += new MouseEventHandler(Idle_MouseDown);
            idleImg.MouseUp += new MouseEventHandler(Idle_MouseUp);
            idleImg.MouseMove += new MouseEventHandler(Idle_MouseMove);
            idleImg.DoubleClick += new EventHandler(Idle_DoubleClick);

            idleImg.Visible = true;


            idleClock = CreateLabel("idleClock", "00:00", new Size(130, 50), new Point(120, 30), Color.White, Color.Transparent,
                new Font("Microsoft YaHei", 30, FontStyle.Regular), true);
            idleClock.Parent = idleImg;

            idleImg.BringToFront();
            idleClock.BringToFront();
        }


        //InPitsV2, BrakeBias, Gear, Fuel, TC
        private void NonRealtimeCalculations(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            fpsCounter = 0;

            InPitsV2(e);
            CalculateGear(e);
            CalculateFuel(e);


            remainingTime = e.TelemetryInfo.SessionTimeRemain.Value;

            #region Set Fuel level

            fuelLevel = e.TelemetryInfo.FuelLevel.Value;
            Fuel_remain_value.Text = string.Format("{0:00.00}", fuelLevel);

            #endregion

            #region EngineWarnings (Stalled engine)

            engine_panel.Visible = engineWarning == 30 ? true : false;

            #endregion


            if (DriverCarIdx != -9999)
                position = e.TelemetryInfo.CarIdxPosition.Value[DriverCarIdx];
        }


        private Panel CreateSettingWindow(Point location, Color color, Size size, bool visible)
        {
            Panel panel = new Panel();
            panel.Location = location;
            panel.BackColor = color;
            panel.Size = size;
            panel.Visible = visible;

            if (!this.Controls.Contains(panel))
                this.Controls.Add(panel);

            return panel;
            //settingsPanel.BringToFront();
        }

        private Label CreateLabel(string name, string text, Size size, Point location, Color foreColor, Color backColor, Font font,
            bool visible)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = location;
            label.Visible = visible;
            label.ForeColor = foreColor;
            label.BackColor = backColor;
            label.Font = font;
            label.Name = name;
            label.Size = size;

            this.Controls.Add(label);

            return label;
        }

        private void WarningFlashes(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            flashingFpsCounter = 0;
            var sessionFlag = e.TelemetryInfo.SessionFlags.Value.ToString();

            warning_panel.BackColor = Color.Transparent;

            if (sessionFlag.Contains("Repair"))
            {
                LightPanel(warning_panel, Color.Black);
            }
            else
            if (sessionFlag.Contains("Blue"))
            {
                LightPanel(warning_panel, Color.Blue);
            }
            else if (sessionFlag.Contains("Yellow") || sessionFlag.Contains("Caution") ||
                     sessionFlag.Contains("CautionWaving") || sessionFlag.Contains("YellowWaving"))
            {
                LightPanel(warning_panel, Color.Yellow);
            }
            else if (sessionFlag.Contains("Green"))
            {
                LightPanel(warning_panel, Color.Green);
            }
            else if (sessionFlag.Contains("White"))
            {
                LightPanel(warning_panel, Color.White);
            }



            if (fuelLevel < 5)
                panel4.BackColor = Color.DarkRed;
            else if (fuelLevel < 10)
                LightPanel(panel4, Color.DarkRed);

            if (engineWarning == 30)
            {
                if (engine_value.Visible)
                    engine_value.Visible = false;
                else
                    engine_value.Visible = true;
            }
        }


        private void LightPanel(Panel panel, Color color)
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

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            try
            {
                fpsCounter++;
                flashingFpsCounter++;
                settingPanelFpsCounter++;

                //Folyamatosan frissülő adatok
                sessionNumber = e.TelemetryInfo.SessionNum.Value;


                if (sessionNumberTemp == -1)
                {
                    //init sessionNum
                    sessionNumberTemp = sessionNumber;
                }


                if (sessionNumberTemp != sessionNumber || subSessionNumber != subSessionNumberTemp)
                {
                    #region Variables reset

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
                    //traction1_value.Text = "N/A";
                    boost_value.Text = "N/A";
                    Laptime_value.Text = "00:00.000";
                    rpm.Text = "N/A";
                    gear.Text = "N";
                    Last_lap_value.Text = "N/A";
                    fuel_to_finish_value.Text = "N/A";
                    initForFuel = true;

                    sessionNumberTemp = sessionNumber;

                    if (newLapLogger == null)
                        newLapLogger = new Logger(logPath + "\\" + dateInString + "\\errorLog.txt");
                    newLapLogger.Log("New session", "");

                    #endregion
                }
                else
                {
                    Speed_value.Text = Math.Round(e.TelemetryInfo.Speed.Value * 3.6).ToString();

                    //újraszámoljuk a szükséges adatokat
                    UpdateRpmLights(e);
                    NewLapCalculation(e);


                    if (fpsCounter == (int)(wrapper.TelemetryUpdateFrequency / NonRTCalculationFPS))
                        NonRealtimeCalculations(e);

                    if (flashingFpsCounter == (int)(wrapper.TelemetryUpdateFrequency / flashingFps))
                        WarningFlashes(e);

                    #region Set boost

                    boost = Int32.Parse(wrapper.GetData("dcBoostLevel").ToString());
                    boost_value.Text = boost.ToString();

                    #endregion

                    #region Set TC1 TC2

                    tractionControl1 = Int32.Parse(wrapper.GetData("dcTractionControl").ToString());
                    tractionControl2 = Int32.Parse(wrapper.GetData("dcTractionControl2").ToString());

                    #endregion

                    #region Set Brake bias

                    brakeBias = float.Parse(wrapper.GetData("dcBrakeBias").ToString());
                    Brake_bias_value.Text = string.Format("{0:00.00}", Convert.ToDouble(brakeBias));

                    #endregion

                    #region Init temp values for car settings

                    if (boostTemp == -1)
                        boostTemp = boost;

                    if (tractionControl1Temp == -1)
                        tractionControl1Temp = tractionControl1;

                    if (tractionControl2Temp == -1)
                        tractionControl2Temp = tractionControl2;

                    if (brakeBiasTemp == -1)
                        brakeBiasTemp = brakeBias;

                    #endregion

                    #region Any setting on car flashes here

                    CarSettingPanelFlash(e, ref boost, ref boostTemp, "BOOST");
                    CarSettingPanelFlash(e, ref tractionControl1, ref tractionControl1Temp, "TC1");
                    CarSettingPanelFlash(e, ref tractionControl2, ref tractionControl2Temp, "TC2");
                    CarSettingPanelFlash(e, ref brakeBias, ref brakeBiasTemp, "FRT BRB");

                    #endregion

                    //az UpdateLapTime mögé kellett rakjam, hogy legyen egy temp kör szám, így az updatelaptimeban majd a legfrissebbel hasonlitja ezt ami előtte bennevolt
                    lapCountTemp = e.TelemetryInfo.Lap.Value;


                    //Textek kiirása
                    rpm.Text = Math.Round(e.TelemetryInfo.RPM.Value).ToString();
                }
            }
            catch (Exception ex)
            {
                if (errorLogger == null)
                    errorLogger = new Logger(logPath + "\\" + dateInString + "\\errorLog.txt");
                errorLogger.Log("OnSessionTelemetryUpdated Error", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void CarSettingPanelFlash<T>(SdkWrapper.TelemetryUpdatedEventArgs e, ref T actualValue, ref T valueTemp,
            string title)
        {
            #region Set Traction Control TC1, TC2, Boost

            try
            {
                if (!actualValue.Equals(valueTemp))
                {
                    if (settingLabelTitle.Visible == false)
                        settingPanelFpsCounter = 0;

                    //panelek megjelenitése
                    if (settingPanelFpsCounter <= (int)(wrapper.TelemetryUpdateFrequency / settingPanelFps))
                    {
                        settingLabelTitle.Text = title;
                        if (actualValue.GetType() == typeof(float))
                        {
                            settingLabelValue.Text = string.Format("{0:0.00}", actualValue);
                        }
                        else
                        {
                            settingLabelValue.Text = actualValue.ToString();
                        }


                        //settingsPanel.Visible = true;
                        settingLabelTitle.Visible = true;
                        settingLabelValue.Visible = true;
                    }
                    else
                    {
                        //settingsPanel.Visible = false;
                        settingLabelTitle.Visible = false;
                        settingLabelValue.Visible = false;
                        settingPanelFpsCounter = 0;
                        valueTemp = actualValue;
                    }
                }
            }
            catch (Exception ex)
            {
                settingLabelTitle.Text = title;
                settingLabelValue.Text = "N/A";
            }

            #endregion
        }

        private void InPitsV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            engineWarning = Int32.Parse(wrapper.GetData("EngineWarnings").ToString());

            var onPitRoad = Convert.ToBoolean(wrapper.GetData("OnPitRoad"));
            var ontrack = Convert.ToBoolean(e.TelemetryInfo.IsOnTrack.Value);

            if ((onPitRoad && initForFuel) || (ontrack && initForFuel))
                initForFuel = false;

            if (engineWarning == 16 || engineWarning == 48) //16 = pit limiter
            {
                Pit_Limiter_title.Visible = onPitRoad ? true : false;
                speed_limiter.Visible = true;
                Pit_limiter_background_panel.Visible = true;
            }
            else
            {
                Pit_Limiter_title.Visible = onPitRoad ? true : false;
                Pit_Limiter_title.Visible = false;
                Pit_limiter_background_panel.Visible = false;
            }
        }

        private void CalculateGear(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            gearValue = e.TelemetryInfo.Gear.Value;
            if (gearValue == -1)
                gear.Text = "R";
            else if (gearValue == 0)
                gear.Text = "N";
            else gear.Text = gearValue.ToString();
        }

        private void NewLapCalculation(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            NewLapSettings(e);
            UpdateLapTimeV2(e);
            CalculateDeltaV2(e);
            CalculateFuelUsagePerLap(e);
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
                newLapLogger = new Logger(logPath + "\\" + dateInString + "\\newLap.txt");
                this.newLapLogger.Log("Init",
                    "lapCountTemp: " + lapCountTemp + Environment.NewLine + "  currentLap: " + currentLap);
            }

            #endregion

            if (lapCount - lapCountTemp == 1)
            {
                var lapTime = Convert.ToDouble(wrapper.GetData("LapLastLapTime"));
                //var lapTimeConverted = Convert.ToDouble(lapTime);
                if (lapTime != 0 && lapTime != -1 && !initForFuel)
                    laptimes.Add(lapTime);

                if (initForFuel)
                    initForFuel = false;

                //laptimes.Add(lapTime);
                lapCountPrevious = currentLap;
                currentLap = lapCount;
                //manualLapCount++;

                newLapLogger.Log("In a new lap",
                    "lapTime: " + lapTime + Environment.NewLine + "  lapCountPrevious: " + lapCountPrevious +
                    Environment.NewLine + "  currentLap: " + currentLap);
            }
        }

        private void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var deltaObject = wrapper.GetData("LapDeltaToSessionBestLap");
            var deltaInt = Convert.ToDouble(deltaObject);

            if (deltaInt > 0 && deltaInt < 99.99)
            {
                delta_panel.BackColor = Color.Firebrick;
                Delta_value.Text = string.Format("{0:+0.00}", deltaInt);
            }
            else if (deltaInt <= 0 && deltaInt > -99.99)
            {
                delta_panel.BackColor = Color.Green;
                Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt == 0)
            {
                delta_panel.BackColor = Color.FromArgb(255, 40, 40, 40);
                Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt >= 99.99)
            {
                delta_panel.BackColor = Color.Firebrick;
                Delta_value.Text = "+99.99";
            }
            else if (deltaInt <= -99.99)
            {
                delta_panel.BackColor = Color.Green;
                Delta_value.Text = "-99.99";
            }
        }

        private void CalculateFuelUsagePerLap(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //új kör esetén
            if (lapCount - lapCountTemp == 1)
            {
                var actualFuelLevel = e.TelemetryInfo.FuelLevel.Value;

                if (fuelLapStart != -1 && fuelLapStart - actualFuelLevel > 0)
                {
                    fuelUsagePerLap.Add(fuelLapStart - actualFuelLevel);
                }

                #region fuelLapStart init

                if (fuelLapStart == -1)
                {
                    fuelLapStart = actualFuelLevel;

                    fuelUsageLogger = new Logger(logPath + "\\" + dateInString + "\\fuelUsage.txt");
                    fuelUsageLogger.Log("Init",
                        "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
                    return;
                }



                #endregion

                var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;
                fuelUsageLogger.Log("Difference", "Lap number: " + lapCount + Environment.NewLine +
                                                  "  lapStartEndFuelDifference: " + lapStartEndFuelDifference +
                                                  Environment.NewLine
                                                  + "  fuelLapStart: " + fuelLapStart + Environment.NewLine +
                                                  "  actualFuelLevel: " + actualFuelLevel);

                Last_lap_value.Text = string.Format("{0:0.00}", lapStartEndFuelDifference);

                fuelLapStart = actualFuelLevel;
            }
            

        }

        private void UpdateLapTimeV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapObject = wrapper.GetData("LapCurrentLapTime");
            var lap = Convert.ToDouble(lapObject);

            int min = (int)(lap / 60);
            int sec = (int)(lap % 60);

            double decimalPart = lap % (int)lap;

            string laptime;
            laptime = decimalPart > 0
                ? string.Format("{0:00}:{1:00}.{2:000}", min, sec, decimalPart.ToString().Substring(2, 3))
                : laptime = string.Format("{0:00}:{1:00}.{2:000}", 0, 0, 0);

            Laptime_value.Text = laptime;
        }

        private void UpdateRpmLights(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var rpm = e.TelemetryInfo.RPM.Value;

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

                #region Init subSessionNumberTemp

                if (subSessionNumberTemp == -1)
                    subSessionNumberTemp = subSessionNumber;

                #endregion

                if (sessionNumber != sessionNumberTemp || subSessionNumber != subSessionNumberTemp)
                {
                    #region Resetting variables

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
                    //traction1_value.Text = "N/A";
                    boost_value.Text = "N/A";
                    Laptime_value.Text = "00:00.000";
                    rpm.Text = "N/A";
                    gear.Text = "N";
                    Last_lap_value.Text = "N/A";
                    initForFuel = true;

                    //newLapLogger.Log("Resetting variables", Environment.NewLine + "OnSessionUpdated");

                    #endregion
                }
                else
                {
                    maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
                    trackLength = float.Parse(e.SessionInfo["WeekendInfo"]["TrackLength"].Value.Substring(0, 4)) * 1000;

                    var isMaxRpmValid = float.TryParse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value,
                        out maxRpm);

                    if (isMaxRpmValid)
                        maxRpm = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value);
                }
            }
            catch (Exception ex)
            {
                if (errorLogger == null)
                    errorLogger = new Logger(logPath + "\\" + dateInString + "\\errorLog.txt");
                errorLogger.Log("OnSessionInfoUpdated Error", ex.Message);
            }
        }

        private void CalculateFuel(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            #region Gathering required variables to calculate fuel

            //required vars
            //remaining time, avg fuel usage, avg laptime
            var avgFuelUsage = GetAverageFromList(fuelUsagePerLap);
            var avgLapTime = GetAverageFromList(laptimes);

            #endregion

            #region Calculate estimate laps

            var remainingLapsWithFuel = fuelLevel / avgFuelUsage;
            if (HasValue(remainingLapsWithFuel))
                Laps_estimate_value.Text = string.Format("{0:0.00}", remainingLapsWithFuel);

            #endregion

            #region Calculate fuel to fill

            var timeWithoutFuel = remainingTime - (avgLapTime * remainingLapsWithFuel);

            if (timeWithoutFuel > 0)
            {
                var minimumLapsToFill = Math.Floor(timeWithoutFuel / avgLapTime);
                var leftOverSeconds = timeWithoutFuel % avgLapTime;

                //calculate fuel for leftover seconds
                var fillForLeftover = (leftOverSeconds / (avgLapTime / 100)) * (avgFuelUsage / 100);

                if (HasValue(minimumLapsToFill) && HasValue(fillForLeftover))
                {
                    var toFill = (minimumLapsToFill * avgFuelUsage) + fillForLeftover + Math.Ceiling(avgFuelUsage) +
                                 1; //+1 liter for safety

                    Fuel_to_fill_value.Text = toFill > maxFuelOfCar
                        ? string.Format("{0:0.00}", maxFuelOfCar)
                        : string.Format("{0:0.00}", toFill);
                }
            }
            else
            {
                Fuel_to_fill_value.Text = "CHCK";
            }

            #endregion

            #region Calculate if fuel is enough/not to finish line respectively

            var avgFuelUsageInASecond = avgFuelUsage / avgLapTime;
            var litersPlusOrMinus = avgFuelUsageInASecond * timeWithoutFuel;
            var toFinishLine = (remainingTime + avgLapTime) * avgFuelUsageInASecond;

            //és a jelenlegi benzinből mennyi jön még ki a diff-re kalkulálva
            var allSecondsTillTankIsEmpty = (remainingLapsWithFuel * avgLapTime) * avgFuelUsageInASecond;

            var diff = allSecondsTillTankIsEmpty - toFinishLine;

            if (diff >= 0)
            {
                fuel_to_finish_value.Text = string.Format("{0:+0.00}", diff);
                fuel_to_finish_value.ForeColor = Color.White;
                panel10.BackColor = Color.LimeGreen;
            }
            else if (diff < 0)
            {
                fuel_to_finish_value.Text = string.Format("{0:0.00}", diff);
                fuel_to_finish_value.ForeColor = Color.White;
                panel10.BackColor = Color.Firebrick;
            }

            #endregion
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

        protected void Idle_MouseDown(object sender, MouseEventArgs e)
        {
            if (!clicked)
                clicked = true;
        }

        protected void Idle_MouseUp(object sender, MouseEventArgs e)
        {
            if (clicked)
                clicked = false;
        }
        protected void Idle_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
                this.Location = new Point(Location.X + e.X, Location.Y + e.Y);
        }

        protected void Idle_DoubleClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}