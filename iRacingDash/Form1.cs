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
using System.Timers;
using System.Windows.Forms;
using iRacingSdkWrapper;
using iRSDKSharp;
using iRacingSdkWrapper.Bitfields;
using iRacingDash.Sessions;
using iRacingDash.Enums;

//EngineWarnings : pits = 16, rpmMaximum = 32, normal = 0, stalled = 30
namespace iRacingDash
{
    public partial class Form1 : Form
    {
        private static DateTime now = DateTime.Now;
        private SdkWrapper _wrapper;
        private iRacingSDK _sdk;


        private delegate void SafeCallDelegate(bool visible, string time);
        private static Configurator s = new Configurator();
        //public static string logPath = s.Configurate<string>("logPath", "config", "Path");
        public static string dateInString = now.ToString().Replace(' ', '-').Replace('/', '-').Replace(':', '-');
        private Logger newLapLogger;
        private Logger fuelUsageLogger;
        private Logger fuelComputeLogger;
        private Logger errorLogger;

        private Point location;
        private int fps;

        //LEDS
        //private float minRpmPercent;

        //private float shiftLight1Percent;
        //private float shiftLight2Percent;
        //private float redLinePercent;

        ////COMMON
        //private int DriverCarIdx = -9999;

        //private int gearValue = 0;

        private float maxRpm;
        //private float lapDist = 0;
        //private int lapCount = 0;
        //private int lapCountTemp = -1;
        //private float lapCountPrevious = 0;
        //private float trackLength = -9999;
        //private int position;
        //private bool initForFuel = true;
        //private bool initLapTime = true;
        //private int engineWarning;
        private string sessionType;
        //private string raceWeek;
        //private string eventType;
        //private string trackName;

        private int NonRTCalculationFPS = s.Configurate<int>("fps", "config", "NonRealtimeCalculationsFps");
        //private int fpsCounter = 0;
        //private int flashingFps = 2;
        //private int flashingFpsCounter = 0;

        

        private static System.Timers.Timer timer1;
        private static int timerTime = -1;
        private bool remainingTimeIsSet = false;


        //*FUEL
        private float maxFuelOfCar = 0;

        private float fuelLevel;
        private int manualLapCount = -1;

        //**ESTIMATED LAPS
        private double remainingTime;
        private double remainingTimeTemp;
        private bool raceOver = false;

        private int sessionNumber;
        private int sessionNumberTemp = -1;

        private int subSessionNumber;
        //private int subSessionNumberTemp = -1;

        private float fuelLapStart = -1;
        private List<double> fuelUsagePerLap = new List<double>();

        //köridők
        List<double> laptimes = new List<double>();
        private double lapTime;
        private string lapTimeString;
        float currentLap;
        private bool raceStarted;
        private float raceStartLap;

        //Delta
        private int deltaLimit = 2;

        


        //Settings on car
        private int boost;
        private int boostTemp = -1;

        private int tractionControl1;
        private int tractionControl1Temp = -1;

        private int tractionControl2;
        private int tractionControl2Temp = -1;

        private float brakeBias;
        private float brakeBiasTemp = -1;

        private int engineMap;
        private int engineMapTemp = -1;

        //

        private DefaultSession defaultSession;
        private Dash dash;
        public Form1()
        {
            InitializeComponent();

            _sdk = new iRacingSDK();
            _wrapper = new SdkWrapper();
            //_wrapper = new iRacingMock.ClassLibrary.Mock("D:\\_20190810_115035.csv");

           //defaultSession = new DefaultSession();
            dash = new Dash(this, _wrapper);
            dash.InitDash();
            new Configurator().StartConfig(ref maxRpm, ref location, ref dash.minRpmPercent, ref dash.shiftLight1Percent, ref dash.shiftLight2Percent, ref dash.redLinePercent, ref fps);

            Location = location;
            _wrapper.TelemetryUpdateFrequency = fps;

            SubscribeToEvents();
            _wrapper.Start();
        }

        public void SubscribeToEvents()
        {
            _wrapper.Connected += _wrapper_Connected;
            //_wrapper.TelemetryUpdated += OnTelemetryUpdated;
            _wrapper.SessionInfoUpdated += OnSessionInfoUpdated;
        }

        private void _wrapper_Connected(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            sessionType = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["SessionType"].Value;
            var sessionTypeEnum = GetEnumValueFromDescription<SessionTypeEnum>(sessionType);
            

            switch (sessionTypeEnum)
            {
                case SessionTypeEnum.OfflineTesting:
                    var offlineTestingSession= new OfflineTestingSession(NonRTCalculationFPS, this, _wrapper, dash);
                    _wrapper.TelemetryUpdated += offlineTestingSession.OnTelemetryUpdated;
                    _wrapper.SessionInfoUpdated += offlineTestingSession.OnSessionInfoUpdated;
                    break;
                case SessionTypeEnum.Practice:
                    var practiceSession = new PracticeSession(NonRTCalculationFPS, this, _wrapper, dash);
                    _wrapper.TelemetryUpdated += practiceSession.OnTelemetryUpdated;
                    _wrapper.SessionInfoUpdated += practiceSession.OnSessionInfoUpdated;
                    break;
                case SessionTypeEnum.Qualify:
                    var qualifySession = new QualifySession(NonRTCalculationFPS, this, _wrapper, dash);
                    _wrapper.TelemetryUpdated += qualifySession.OnTelemetryUpdated;
                    _wrapper.SessionInfoUpdated += qualifySession.OnSessionInfoUpdated;
                    break;
                case SessionTypeEnum.WarmUp:
                    break;
                case SessionTypeEnum.Race:
                    break;
                default:
                    break;
            }
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            MemberInfo[] fis = typeof(T).GetFields();

            foreach (var fi in fis)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0 && attributes[0].Description == description)
                    return (T)Enum.Parse(typeof(T), fi.Name);
            }

            throw new Exception("Not found");
        }


        private void NewLapCalculation(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //NewLapSettings(e);
            //UpdateLapTimeV2(e);
            //CalculateDeltaV2(e);
            //CalculateFuelUsagePerLap(e);
        }

        //private void NewLapSettings(SdkWrapper.TelemetryUpdatedEventArgs e)
        //{
        //    lapDist = e.TelemetryInfo.LapDist.Value;
        //    lapCount = e.TelemetryInfo.Lap.Value;

        //    #region Init lapCountTemp

        //    if (lapCountTemp == -1)
        //    {
        //        lapCountTemp = lapCount;
        //        currentLap = lapCount;

        //        //creating the logger here
        //        //fuelComputeLogger = new Logger(logPath + dateInString + "\\fuelCompute.txt"); TODO
        //        newLapLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\newLap.txt");
        //        this.newLapLogger.Log("Init",
        //            "lapCountTemp: " + lapCountTemp + Environment.NewLine + "  currentLap: " + currentLap);
        //    }

        //    #endregion

        //    if (lapCount - lapCountTemp == 1)
        //    {
        //        //this is for the first launch, to not store biiig laptimes
        //        if (initLapTime)
        //        {
        //            initLapTime = false;
        //        }
        //        else
        //        {
        //            new Thread(() =>
        //            {
        //                Thread.Sleep(2000);
        //                lapTimeString = _wrapper.GetData("LapLastLapTime").ToString();
        //                lapTime = Convert.ToDouble(lapTimeString);

        //                if (lapTime != 0 && lapTime != -1 && !initForFuel)
        //                    laptimes.Add(lapTime);

        //                if (initForFuel)
        //                    initForFuel = false;

        //                lapCountPrevious = currentLap;
        //                currentLap = lapCount;

        //                newLapLogger.Log("In a new lap",
        //                    "lapTime: " + lapTime +
        //                    Environment.NewLine + "  lapCountPrevious: " + lapCountPrevious +
        //                    Environment.NewLine + "  currentLap: " + currentLap +
        //                    Environment.NewLine + "  initLapTime: " + initLapTime +
        //                    //Environment.NewLine + "  lapTimeString: " + lapTimeString +
        //                    //Environment.NewLine + "  lapTime: " + lapTime +
        //                    Environment.NewLine + "  initForFuel: " + initForFuel);

        //            }).Start();
        //        }

        //    }
        //}

        //private void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        //{
        //    var deltaObject = _wrapper.GetData("LapDeltaToSessionBestLap");
        //    var deltaInt = Convert.ToDouble(deltaObject);

        //    if (deltaInt > 0 && deltaInt < 99.99)
        //    {
        //        delta_panel.BackColor = Color.Firebrick;
        //        Delta_value.Text = string.Format("{0:+0.00}", deltaInt);
        //    }
        //    else if (deltaInt < 0 && deltaInt > -99.99)
        //    {
        //        delta_panel.BackColor = Color.Green;
        //        Delta_value.Text = string.Format("{0:0.00}", deltaInt);
        //    }
        //    else if (deltaInt == 0)
        //    {
        //        delta_panel.BackColor = Color.FromArgb(255, 40, 40, 40);
        //        Delta_value.Text = string.Format("{0:0.00}", deltaInt);
        //    }
        //    else if (deltaInt >= 99.99)
        //    {
        //        delta_panel.BackColor = Color.Firebrick;
        //        Delta_value.Text = "+99.99";
        //    }
        //    else if (deltaInt <= -99.99)
        //    {
        //        delta_panel.BackColor = Color.Green;
        //        Delta_value.Text = "-99.99";
        //    }
        //}

        //private void CalculateFuelUsagePerLap(SdkWrapper.TelemetryUpdatedEventArgs e)
        //{
        //    //új kör esetén
        //    if (lapCount - lapCountTemp == 1)
        //    {
        //        var actualFuelLevel = e.TelemetryInfo.FuelLevel.Value;

        //        if (fuelLapStart != -1 && fuelLapStart - actualFuelLevel > 2)
        //        {
        //            if (true) //sessionType == "Race"
        //            {
        //                if (lapCount > 2) //lapCount >= raceStartLap
        //                    fuelUsagePerLap.Add(fuelLapStart - actualFuelLevel);
        //            }
        //            else
        //            {
        //                fuelUsagePerLap.Add(fuelLapStart - actualFuelLevel);
        //            }



        //        }

        //        #region fuelLapStart init

        //        if (fuelLapStart == -1)
        //        {
        //            fuelLapStart = actualFuelLevel;

        //            fuelUsageLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\fuelUsage.txt");
        //            fuelUsageLogger.Log("Init",
        //                "Lap number:  " + lapCount + Environment.NewLine + "  fuelLapStart: " + fuelLapStart);
        //            return;
        //        }



        //        #endregion

        //        var lapStartEndFuelDifference = fuelLapStart - actualFuelLevel;

        //        if (lapStartEndFuelDifference > 0)
        //        {
        //            Last_lap_value.Text = string.Format("{0:0.00}", lapStartEndFuelDifference);
        //            Last_lap_value.ForeColor = Color.FromArgb(255, 128, 0);
        //        }
        //        else
        //        {
        //            Last_lap_value.ForeColor = Color.Yellow;
        //        }


        //        fuelUsageLogger.Log("Difference", "Lap number: " + lapCount +
        //                                          Environment.NewLine + "  fuelLapStart: " + fuelLapStart +
        //                                          Environment.NewLine + "  actualFuelLevel: " + actualFuelLevel +
        //                                          Environment.NewLine + "  lapStartEndFuelDifference: " +
        //                                          lapStartEndFuelDifference +
        //                                          Environment.NewLine + "  fuelLapStart is set to actualFuelLevel" +
        //                                          Environment.NewLine + "FuelUsagePerLap: " + string.Join(",", fuelUsagePerLap));

        //        fuelLapStart = actualFuelLevel;
        //    }


        //}

        private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            sessionType = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["SessionType"].Value;
        }

        //private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        //{
        //    try
        //    {
        //        subSessionNumber = Int32.Parse(e.SessionInfo["WeekendInfo"]["SubSessionID"].Value);

        //        #region Init subSessionNumberTemp

        //        if (subSessionNumberTemp == -1)
        //            subSessionNumberTemp = subSessionNumber;

        //        #endregion

        //        if (sessionNumber != sessionNumberTemp || subSessionNumber != subSessionNumberTemp)
        //        {
        //            #region Resetting variables

        //            maxFuelOfCar = -9999;
        //            trackLength = 0;
        //            subSessionNumberTemp = subSessionNumber;

        //            lapCount = 0;
        //            lapCountTemp = -1;
        //            lapCountPrevious = 0;

        //            fuelLapStart = -1;
        //            fuelUsagePerLap.Clear();
        //            DriverCarIdx = -9999;
        //            remainingTime = 0;
        //            fuelLevel = 0;
        //            trackLength = -9999;

        //            Fuel_to_fill_value.Text = "N/A";
        //            Fuel_remain_value.Text = "N/A";
        //            Laps_estimate_value.Text = "N/A";
        //            Brake_bias_value.Text = "N/A";
        //            Speed_value.Text = "N/A";
        //            Delta_value.Text = "N/A";
        //            //traction1_value.Text = "N/A";
        //            boost_value.Text = "N/A";
        //            Laptime_value.Text = "00:00.000";
        //            rpm.Text = "N/A";
        //            gear.Text = "N";
        //            Last_lap_value.Text = "N/A";
        //            initForFuel = true;

        //            //newLapLogger.Log("Resetting variables", Environment.NewLine + "OnSessionUpdated");

        //            #endregion
        //        }
        //        else
        //        {
        //            maxFuelOfCar = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarFuelMaxLtr"].Value);
        //            trackLength = float.Parse(e.SessionInfo["WeekendInfo"]["TrackLength"].Value.Substring(0, 4)) * 1000;

        //            var isMaxRpmValid = float.TryParse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value,
        //                out maxRpm);

        //            if (isMaxRpmValid)
        //                maxRpm = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value);

        //            sessionType = e.SessionInfo["SessionInfo"]["Sessions"]["SessionNum", sessionNumber]["SessionType"].Value;
        //            raceWeek = e.SessionInfo["WeekendInfo"]["RaceWeek"].Value;
        //            eventType = e.SessionInfo["WeekendInfo"]["EventType"].Value;
        //            eventType = eventType.Replace(' ', '_');
        //            trackName = e.SessionInfo["WeekendInfo"]["TrackName"].Value;
        //            trackName = trackName.Replace(' ', '_');
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (errorLogger == null)
        //            errorLogger = new Logger(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\iRacingDash\\logs\\iRacingDash\\" + dateInString + "_W" + raceWeek + "_" + eventType + "_" + trackName + "\\errorLog.txt");
        //        errorLogger.Log("OnSessionInfoUpdated Error", ex.Message);
        //    }
        //}

        //private void CalculateFuel(SdkWrapper.TelemetryUpdatedEventArgs e)
        //{
        //    #region Gathering required variables to calculate fuel
        //    //FOR FUEL TO FINISH - OVERWRITE REMAININGTIME FOR TIMERTIME
        //    if (timerTime != -1 && raceOver)
        //        remainingTime = timerTime;

        //    //required vars
        //    //remaining time, avg fuel usage, avg laptime
        //    var avgFuelUsage = GetAverageFromList(fuelUsagePerLap);
        //    var avgLapTime = GetAverageFromList(laptimes);

        //    #endregion

        //    #region Calculate estimate laps

        //    var remainingLapsWithFuel = fuelLevel / avgFuelUsage;
        //    if (HasValue(remainingLapsWithFuel))
        //        Laps_estimate_value.Text = string.Format("{0:0.00}", remainingLapsWithFuel);

        //    #endregion

        //    #region Calculate fuel to fill

        //    var timeWithoutFuel = remainingTime - (avgLapTime * remainingLapsWithFuel);

        //    if (timeWithoutFuel > 0)
        //    {
        //        var minimumLapsToFill = Math.Floor(timeWithoutFuel / avgLapTime);
        //        var leftOverSeconds = timeWithoutFuel % avgLapTime;

        //        //calculate fuel for leftover seconds
        //        var fillForLeftover = (leftOverSeconds / (avgLapTime / 100)) * (avgFuelUsage / 100);

        //        if (HasValue(minimumLapsToFill) && HasValue(fillForLeftover))
        //        {
        //            var toFill = (minimumLapsToFill * avgFuelUsage) + fillForLeftover + Math.Ceiling(avgFuelUsage) +
        //                         1; //+1 liter for safety

        //            Fuel_to_fill_value.Text = toFill > maxFuelOfCar
        //                ? string.Format("{0:0.00}", maxFuelOfCar)
        //                : string.Format("{0:0.00}", toFill);
        //        }
        //    }
        //    else
        //    {
        //        Fuel_to_fill_value.Text = "CHCK";
        //    }

        //    if (!HasValue(timeWithoutFuel))
        //        Fuel_to_fill_value.Text = "N/A";

        //    #endregion

        //    #region Calculate if fuel is enough/not to finish line respectively
        //    //if (remainingTimeTemp > 1 && raceOver && !remainingTimeIsSet)
        //    //{
        //    //    remainingTime = avgLapTime;
        //    //    remainingTimeIsSet = true;
        //    //}

        //    if (timerTime != -1)
        //    {
        //        CalculateFuelTillFinishLine(timerTime, avgFuelUsage, avgLapTime, timeWithoutFuel,
        //            remainingLapsWithFuel);
        //    }
        //    else
        //    {
        //        CalculateFuelTillFinishLine(remainingTime, avgFuelUsage, avgLapTime, timeWithoutFuel,
        //            remainingLapsWithFuel);
        //    }


        //    //átlagos eset, ha az óra lejárt VISZONT kezdéskor is végig -1 ezért azt is kezelni kell
        //    if (lapCount > 2)
        //    {
        //        if (remainingTime != -1)
        //        {
        //            remainingTimeTemp = remainingTime;
        //        }
        //        else
        //        {
        //            raceOver = true;
        //            timerTime = (int)avgLapTime;
        //            remainingTime = avgLapTime;
        //            //start custom timer
        //            timer1 = new System.Timers.Timer();
        //            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);
        //            timer1.Interval = 1000; // 1 second
        //            timer1.Start();

        //        }
        //    }


        //    #endregion
        //}

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    timerTime--;
        //    remainingTime = timerTime;
        //    if (timerTime == 0)
        //        timer1.Stop();
        //}

        //private void CalculateFuelTillFinishLine(double remainingTime, double avgFuelUsage, double avgLapTime, double timeWithoutFuel, double remainingLapsWithFuel)
        //{

        //    var avgFuelUsageInASecond = avgFuelUsage / avgLapTime;
        //    var litersPlusOrMinus = avgFuelUsageInASecond * timeWithoutFuel;
        //    var toFinishLine = (remainingTime + avgLapTime) * avgFuelUsageInASecond;

        //    //és a jelenlegi benzinből mennyi jön még ki a diff-re kalkulálva
        //    var allSecondsTillTankIsEmpty = (remainingLapsWithFuel * avgLapTime) * avgFuelUsageInASecond;

        //    var diff = allSecondsTillTankIsEmpty - toFinishLine;

        //    if (diff >= 0)
        //    {
        //        fuel_to_finish_value.Text = string.Format("{0:+0.00}", diff);
        //        fuel_to_finish_value.ForeColor = Color.White;
        //        panel10.BackColor = Color.LimeGreen;
        //    }
        //    else if (diff < 0)
        //    {
        //        fuel_to_finish_value.Text = string.Format("{0:0.00}", diff);
        //        fuel_to_finish_value.ForeColor = Color.White;
        //        panel10.BackColor = Color.Firebrick;
        //    }
        //}

        //// Or IsNanOrInfinity
        //public bool HasValue(double value)
        //{
        //    return !Double.IsNaN(value) && !Double.IsInfinity(value);
        //}

        //private double GetAverageFromList(List<double> list)
        //{
        //    var max = 0.0d;
        //    foreach (var value in list)
        //    {
        //        max += value;
        //    }
        //    return max / list.Count;
        //}

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

        public void Idle_MouseDown(object sender, MouseEventArgs e)
        {
            if (!clicked)
                clicked = true;
        }

        public void Idle_MouseUp(object sender, MouseEventArgs e)
        {
            if (clicked)
                clicked = false;
        }
        public void Idle_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
                this.Location = new Point(Location.X + e.X, Location.Y + e.Y);
        }

        public void Idle_DoubleClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}