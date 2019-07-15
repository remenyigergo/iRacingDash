using iRacingSdkWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WECOverlay
{
    public partial class Form1 : Form
    {
        //Instantiates
        private SdkWrapper wrapper;

        Turn turn = new Turn();
        private static Configurator s = new Configurator();

        //Common
        private int driverCarIdx;

        private int carNumber;
        private string teamName;
        private string carClassColor;

        //Turn numbers
        private bool trackSet = false;

        private int trackId = -1;
        private int trackIdTemp = -1;
        SortedList<int, int> Turns = new SortedList<int, int>();

        //Configs
        private bool RecceMode = s.Configurate<bool>("RecceMode", "config", "RecceMode");

        private string classImagesPath = s.Configurate<string>("classImages", "config", "Path");

        //Session
        private int sessionNumber;

        private int sessionNumberTemp = -1;

        private int subSessionNumber;
        private int subSessionNumberTemp = -1;

        private int nonRtFpsCounter = 0;
        private int nonRtFps = 10;

        //RPM / LEDS
        int rpm1 = 2000;

        int rpm2 = 3000;
        int rpm3 = 4000;
        int rpm4 = 5000;
        int rpm5 = 6000;
        int rpm6 = 7000;
        private bool pictureLedEnabled = false;
        private float actual_rpm;
        private float maxRpm = 7300;

        public Form1()
        {
            InitializeComponent();

            Init();

            turn.Visible = true;

            wrapper = new SdkWrapper();
            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 60;
            wrapper.TelemetryUpdated += OnTelemetryUpdated;
            wrapper.SessionInfoUpdated += OnSessionInfoUpdated;
        }

        private void Init()
        {
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;
            pictureBox9.Visible = false;
            pictureBox10.Visible = false;
            pictureBox11.Visible = false;
            pictureBox12.Visible = false;
            pictureBox13.Visible = false;
            pictureBox14.Visible = false;
            pictureBox15.Visible = false;
            pictureBox16.Visible = false;
            pictureBox17.Visible = false;
            pictureBox18.Visible = false;
            pictureBox19.Visible = false;
            pictureBox20.Visible = false;
            pictureBox21.Visible = false;

            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;
            panel8.Visible = false;
            panel9.Visible = false;
            panel10.Visible = false;
            panel11.Visible = false;
            panel12.Visible = false;
            panel13.Visible = false;
            panel14.Visible = false;
            panel15.Visible = false;
            panel16.Visible = false;
            panel17.Visible = false;
            panel18.Visible = false;
            panel19.Visible = false;
            panel20.Visible = false;
            panel21.Visible = false;
            panel22.Visible = false;
            panel23.Visible = false;


            throttle.Size = new Size(12, 33);
            brake.Size = new Size(18, 39);
        }

        private void NonRtCalculation(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            nonRtFpsCounter = 0;

            var distance = e.TelemetryInfo.LapDist.Value;

            if (!RecceMode)
            {
                if (Turns != null && Turns.Count > 0)
                {
                    try
                    {
                        //ezen gyorsítani kellene valamilyen quicksearch-el.
                        int? turnNumber = Turns.First(x => distance < x.Value).Key;
                        turn.label2.Text = (turnNumber - 1).ToString();
                    }
                    catch (Exception ex)
                    {
                        turn.label2.Text = Turns.Keys[Turns.Count - 1].ToString();
                    }
                }
            }
            else
            {
                turn.label1.Font = new Font("Rawhide Raw 2016", 15, FontStyle.Regular);
                turn.label2.Font = new Font("Rawhide Raw 2016", 20, FontStyle.Regular);
                turn.label1.Text = "Distance";
                turn.label2.Text = Convert.ToInt32(distance) + " m";
            }
        }

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            try
            {
                nonRtFpsCounter++;

                //Folyamatosan frissülő adatok
                sessionNumber = e.TelemetryInfo.SessionNum.Value;

                if (sessionNumberTemp == -1)
                {
                    //init sessionNum
                    sessionNumberTemp = sessionNumber;
                }

                if (sessionNumberTemp != sessionNumber || subSessionNumber != subSessionNumberTemp)
                {
                    try
                    {
                        
                        sessionNumberTemp = sessionNumber;
                    }
                    catch (Exception ex)
                    {
                    }
                }

                //ha pályát váltunk, de mondjuk csak practiceben váltjuk a sessiont akkor a sessionNUm nem változik
                if (trackId != trackIdTemp)
                {
                    Turns.Clear();
                    trackSet = false;
                    //trackId = -1;
                }


                CalculateGear(e);
                var speedinKmh = (int) (e.TelemetryInfo.Speed.Value * 3.6);
                speed_value.Text = speedinKmh.ToString();
                sessionNumber = e.TelemetryInfo.SessionNum.Value;
                position.Text = e.TelemetryInfo.CarIdxClassPosition.Value[driverCarIdx].ToString();

                //Kanyarok initje
                //kell az új session resetje
                if (!trackSet && trackId != trackIdTemp && trackId > 0)
                {
                    Turns = new CSVParser().Parse(@"C:\tracksList.csv", trackId);
                    trackSet = true;
                    trackIdTemp = trackId;
                }

                //fps szabályozás a kanyar számokra
                if (nonRtFpsCounter == wrapper.TelemetryUpdateFrequency / nonRtFps)
                    NonRtCalculation(e);

                //adott classon belüli kép megjelenitése
                SetClassColor(e);

                //2 mód van benne. A ledek vagy képekből vagy panelekből épülnek fel
                if (pictureLedEnabled)
                {
                    ShiftLights(e);
                }
                else
                {
                    ShiftLightsPanels(e);
                }

                //Pedálok két képe mozgatása
                Pedals(e);
            }
            catch (Exception ex)
            {
            }
        }

        private void SetClassColor(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            if (classImagesPath.Length != 0)
            {
                switch (carClassColor)
                {
                    case "0xffda59":
                        class_picture.Image = Image.FromFile(classImagesPath + "\\class_ffda59.png");
                        break;
                    case "0x33ceff":
                        class_picture.Image = Image.FromFile(classImagesPath + "\\class_33ceff.png");
                        break;
                    case "0xff5888":
                        class_picture.Image = Image.FromFile(classImagesPath + "\\class_ff5888.png");
                        break;
                    default:
                        class_picture.Image = Image.FromFile(classImagesPath + "\\class_default.png");
                        break;
                }
            }
        }

        private void Pedals(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //max 140 px, min 12
            var throttlePercent = float.Parse(e.TelemetryInfo.Throttle.Value.ToString());
            //max 135, min 18
            var brakePercent = float.Parse(e.TelemetryInfo.Brake.Value.ToString());


            //throttle max - min
            var throttleUnit = 140 - 12;
            var brakeUnit = 135 - 18;

            var widthThrottle = (throttleUnit / 100) * throttlePercent;
            var widthBrake = (brakeUnit / 100) * brakePercent;

            throttle.Size = new Size(12 + (int) (throttleUnit * widthThrottle), 33);
            brake.Size = new Size(18 + (int) (brakeUnit * widthBrake), 39);
        }

        private void CalculateGear(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            //correct print
            var gearValue = e.TelemetryInfo.Gear.Value;
            if (gearValue == -1)
                gear_value.Text = "R";
            else if (gearValue == 0)
                gear_value.Text = "N";
            else gear_value.Text = gearValue.ToString();
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

                if (sessionNumberTemp != sessionNumber || subSessionNumber != subSessionNumberTemp)
                {
                    
                    subSessionNumberTemp = subSessionNumber;
                }

                //sima pályaváltáskor
                if (trackId != trackIdTemp)
                {
                    if(Turns != null)
                        Turns.Clear();
                    trackSet = false;
                    //trackId = -1;
                }

                trackId = Int32.Parse(e.SessionInfo["WeekendInfo"]["TrackID"].Value);

                driverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
                carNumber = Int32.Parse(e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["CarNumber"]
                    .Value);
                teamName = e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["TeamName"].Value;
                carClassColor = e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["CarClassColor"].Value;
                //var telemetryDiskFile = e.SessionInfo["WeekendInfo"]["TelemetryOptions"]["TelemetryDiskFile"].Value;
                var isMaxRpmValid = float.TryParse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value, out maxRpm);

                if (isMaxRpmValid)
                    maxRpm = float.Parse(e.SessionInfo["DriverInfo"]["DriverCarRedLine"].Value);


                car_number_value.Text = carNumber.ToString();
                team_name_value.Text = teamName;
            }
            catch (Exception ex)
            {
            }
        }

        private void ShiftLights(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            actual_rpm = float.Parse(e.TelemetryInfo.RPM.Value.ToString());

            //0-2 x 1000
            if (actual_rpm > 0 && actual_rpm <= rpm1)
            {
                //alsó korlát
                var rpmBottom = 0;
                //felső korlát
                var rpmTop = rpm1;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);
                //felosztás
                var oneFifth = diff / 5;

                //
                var percent1 = (rpmBottom + oneFifth) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneFifth * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneFifth * 3) / (maxRpm / 100);
                var percent4 = (rpmBottom + oneFifth * 4) / (maxRpm / 100);
                var percent5 = (rpmBottom + oneFifth * 5) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent > rpmBottom)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = false;
                    pictureBox3.Visible = false;
                    pictureBox4.Visible = false;
                    pictureBox5.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = true;
                    pictureBox3.Visible = false;
                    pictureBox4.Visible = false;
                    pictureBox5.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = true;
                    pictureBox3.Visible = true;
                    pictureBox4.Visible = false;
                    pictureBox5.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = true;
                    pictureBox3.Visible = true;
                    pictureBox4.Visible = true;
                    pictureBox5.Visible = false;
                }
                else if (rpmPercent > percent4 && rpmPercent < percent5)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = true;
                    pictureBox3.Visible = true;
                    pictureBox4.Visible = true;
                    pictureBox5.Visible = true;
                }

                pictureBox6.Visible = false;
                pictureBox7.Visible = false;
                pictureBox8.Visible = false;
                pictureBox9.Visible = false;
                pictureBox10.Visible = false;
                pictureBox11.Visible = false;
                pictureBox12.Visible = false;
                pictureBox13.Visible = false;
                pictureBox14.Visible = false;
                pictureBox15.Visible = false;
                pictureBox16.Visible = false;
                pictureBox17.Visible = false;
                pictureBox18.Visible = false;
                pictureBox19.Visible = false;
                pictureBox20.Visible = false;
                pictureBox21.Visible = false;
            }
            else if (actual_rpm > 2000 && actual_rpm <= rpm2)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;

                //alsó korlát
                var rpmBottom = rpm1;
                //felső korlát
                var rpmTop = rpm2;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    pictureBox6.Visible = true;
                    pictureBox7.Visible = false;
                    pictureBox8.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox6.Visible = true;
                    pictureBox7.Visible = true;
                    pictureBox8.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox6.Visible = true;
                    pictureBox7.Visible = true;
                    pictureBox8.Visible = true;
                }

                pictureBox9.Visible = false;
                pictureBox10.Visible = false;
                pictureBox11.Visible = false;
                pictureBox12.Visible = false;
                pictureBox13.Visible = false;
                pictureBox14.Visible = false;
                pictureBox15.Visible = false;
                pictureBox16.Visible = false;
                pictureBox17.Visible = false;
                pictureBox18.Visible = false;
                pictureBox19.Visible = false;
                pictureBox20.Visible = false;
                pictureBox21.Visible = false;
            }
            else if (actual_rpm > 3000 && actual_rpm <= rpm3)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;

                //alsó korlát
                var rpmBottom = rpm2;
                //felső korlát
                var rpmTop = rpm3;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    pictureBox9.Visible = true;
                    pictureBox10.Visible = false;
                    pictureBox11.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox9.Visible = true;
                    pictureBox10.Visible = true;
                    pictureBox11.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox9.Visible = true;
                    pictureBox10.Visible = true;
                    pictureBox11.Visible = true;
                }


                pictureBox12.Visible = false;
                pictureBox13.Visible = false;
                pictureBox14.Visible = false;
                pictureBox15.Visible = false;
                pictureBox16.Visible = false;
                pictureBox17.Visible = false;
                pictureBox18.Visible = false;
                pictureBox19.Visible = false;
                pictureBox20.Visible = false;
                pictureBox21.Visible = false;
            }
            else if (actual_rpm > 4000 && actual_rpm <= rpm4)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;
                pictureBox9.Visible = true;
                pictureBox10.Visible = true;
                pictureBox11.Visible = true;

                //alsó korlát
                var rpmBottom = rpm3;
                //felső korlát
                var rpmTop = rpm4;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);
                //felosztás
                var oneFifth = diff / 4;

                //
                var percent1 = (rpmBottom + oneFifth) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneFifth * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneFifth * 3) / (maxRpm / 100);
                var percent4 = (rpmBottom + oneFifth * 4) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    pictureBox12.Visible = true;
                    pictureBox13.Visible = false;
                    pictureBox14.Visible = false;
                    pictureBox15.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox12.Visible = true;
                    pictureBox13.Visible = true;
                    pictureBox14.Visible = false;
                    pictureBox15.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox12.Visible = true;
                    pictureBox13.Visible = true;
                    pictureBox14.Visible = true;
                    pictureBox15.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    pictureBox12.Visible = true;
                    pictureBox13.Visible = true;
                    pictureBox14.Visible = true;
                    pictureBox15.Visible = true;
                }

                pictureBox16.Visible = false;
                pictureBox17.Visible = false;
                pictureBox18.Visible = false;
                pictureBox19.Visible = false;
                pictureBox20.Visible = false;
                pictureBox21.Visible = false;
            }
            else if (actual_rpm > 5000 && actual_rpm <= rpm5)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;
                pictureBox9.Visible = true;
                pictureBox10.Visible = true;
                pictureBox11.Visible = true;
                pictureBox12.Visible = true;
                pictureBox13.Visible = true;
                pictureBox14.Visible = true;
                pictureBox15.Visible = true;

                //alsó korlát
                var rpmBottom = rpm4;
                //felső korlát
                var rpmTop = rpm5;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    pictureBox16.Visible = true;
                    pictureBox17.Visible = false;
                    pictureBox18.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox16.Visible = true;
                    pictureBox17.Visible = true;
                    pictureBox18.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox16.Visible = true;
                    pictureBox17.Visible = true;
                    pictureBox18.Visible = true;
                }

                pictureBox19.Visible = false;
                pictureBox20.Visible = false;
                pictureBox21.Visible = false;
            }
            else if (actual_rpm > 6000 && actual_rpm <= rpm6)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;
                pictureBox9.Visible = true;
                pictureBox10.Visible = true;
                pictureBox11.Visible = true;
                pictureBox12.Visible = true;
                pictureBox13.Visible = true;
                pictureBox14.Visible = true;
                pictureBox15.Visible = true;
                pictureBox16.Visible = true;
                pictureBox17.Visible = true;
                pictureBox18.Visible = true;

                //alsó korlát
                var rpmBottom = rpm5;
                //felső korlát
                var rpmTop = rpm6;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    pictureBox19.Visible = true;
                    pictureBox20.Visible = false;
                    pictureBox21.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    pictureBox19.Visible = true;
                    pictureBox20.Visible = true;
                    pictureBox21.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    pictureBox19.Visible = true;
                    pictureBox20.Visible = true;
                    pictureBox21.Visible = true;
                }
            }
            else if (actual_rpm > rpm6)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;
                pictureBox9.Visible = true;
                pictureBox10.Visible = true;
                pictureBox11.Visible = true;
                pictureBox12.Visible = true;
                pictureBox13.Visible = true;
                pictureBox14.Visible = true;
                pictureBox15.Visible = true;
                pictureBox16.Visible = true;
                pictureBox17.Visible = true;
                pictureBox18.Visible = true;
                pictureBox19.Visible = true;
                pictureBox20.Visible = true;
                pictureBox21.Visible = true;
            }
        }

        private void ShiftLightsPanels(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            actual_rpm = float.Parse(e.TelemetryInfo.RPM.Value.ToString());

            //0-2 x 1000
            if (actual_rpm > 0 && actual_rpm <= rpm1)
            {
                //alsó korlát
                var rpmBottom = 0;
                //felső korlát
                var rpmTop = rpm1;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);
                //felosztás
                var oneSixth = diff / 6;

                //
                var percent1 = (rpmBottom + oneSixth) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneSixth * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneSixth * 3) / (maxRpm / 100);
                var percent4 = (rpmBottom + oneSixth * 4) / (maxRpm / 100);
                var percent5 = (rpmBottom + oneSixth * 5) / (maxRpm / 100);
                var percent6 = (rpmBottom + oneSixth * 5) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent > rpmBottom)
                {
                    panel1.Visible = true;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    panel4.Visible = false;
                    panel5.Visible = false;
                    panel6.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel3.Visible = false;
                    panel4.Visible = false;
                    panel5.Visible = false;
                    panel6.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel3.Visible = true;
                    panel4.Visible = false;
                    panel5.Visible = false;
                    panel6.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel3.Visible = true;
                    panel4.Visible = true;
                    panel5.Visible = false;
                    panel6.Visible = false;
                }
                else if (rpmPercent > percent4 && rpmPercent < percent5)
                {
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel3.Visible = true;
                    panel4.Visible = true;
                    panel5.Visible = true;
                    panel6.Visible = false;
                }
                else if (rpmPercent > percent4 && rpmPercent < percent5)
                {
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel3.Visible = true;
                    panel4.Visible = true;
                    panel5.Visible = true;
                    panel6.Visible = true;
                }

                panel7.Visible = false;
                panel8.Visible = false;
                panel9.Visible = false;
                panel10.Visible = false;
                panel11.Visible = false;
                panel12.Visible = false;
                panel13.Visible = false;
                panel14.Visible = false;
                panel15.Visible = false;
                panel16.Visible = false;
                panel17.Visible = false;
                panel18.Visible = false;
                panel19.Visible = false;
                panel20.Visible = false;
                panel21.Visible = false;
                panel22.Visible = false;
                panel23.Visible = false;
            }
            else if (actual_rpm > 2000 && actual_rpm <= rpm2)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;

                //alsó korlát
                var rpmBottom = rpm1;
                //felső korlát
                var rpmTop = rpm2;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    panel7.Visible = true;
                    panel8.Visible = false;
                    panel9.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel7.Visible = true;
                    panel8.Visible = true;
                    panel9.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel7.Visible = true;
                    panel8.Visible = true;
                    panel9.Visible = true;
                }

                panel10.Visible = false;
                panel11.Visible = false;
                panel12.Visible = false;
                panel13.Visible = false;
                panel14.Visible = false;
                panel15.Visible = false;
                panel16.Visible = false;
                panel17.Visible = false;
                panel18.Visible = false;
                panel19.Visible = false;
                panel20.Visible = false;
                panel21.Visible = false;
                panel22.Visible = false;
                panel23.Visible = false;
            }
            else if (actual_rpm > 3000 && actual_rpm <= rpm3)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;
                panel7.Visible = true;
                panel8.Visible = true;
                panel9.Visible = true;

                //alsó korlát
                var rpmBottom = rpm2;
                //felső korlát
                var rpmTop = rpm3;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneFourth = diff / 4;

                //
                var percent1 = (rpmBottom + oneFourth) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneFourth * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneFourth * 3) / (maxRpm / 100);
                var percent4 = (rpmBottom + oneFourth * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    panel10.Visible = true;
                    panel11.Visible = false;
                    panel12.Visible = false;
                    panel13.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel10.Visible = true;
                    panel11.Visible = true;
                    panel12.Visible = false;
                    panel13.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel10.Visible = true;
                    panel11.Visible = true;
                    panel12.Visible = true;
                    panel13.Visible = true;
                }


                panel14.Visible = false;
                panel15.Visible = false;
                panel16.Visible = false;
                panel17.Visible = false;
                panel18.Visible = false;
                panel19.Visible = false;
                panel20.Visible = false;
                panel21.Visible = false;
                panel22.Visible = false;
                panel23.Visible = false;
            }
            else if (actual_rpm > 4000 && actual_rpm <= rpm4)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;
                panel7.Visible = true;
                panel8.Visible = true;
                panel9.Visible = true;
                panel10.Visible = true;
                panel11.Visible = true;
                panel12.Visible = true;
                panel13.Visible = true;

                //alsó korlát
                var rpmBottom = rpm3;
                //felső korlát
                var rpmTop = rpm4;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);
                //felosztás
                var oneFourth = diff / 4;

                //
                var percent1 = (rpmBottom + oneFourth) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneFourth * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneFourth * 3) / (maxRpm / 100);
                var percent4 = (rpmBottom + oneFourth * 4) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    panel14.Visible = true;
                    panel15.Visible = false;
                    panel16.Visible = false;
                    panel17.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel14.Visible = true;
                    panel15.Visible = true;
                    panel16.Visible = false;
                    panel17.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel14.Visible = true;
                    panel15.Visible = true;
                    panel16.Visible = true;
                    panel17.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    panel14.Visible = true;
                    panel15.Visible = true;
                    panel16.Visible = true;
                    panel17.Visible = true;
                }

                panel18.Visible = false;
                panel19.Visible = false;
                panel20.Visible = false;
                panel21.Visible = false;
                panel22.Visible = false;
                panel23.Visible = false;
            }
            else if (actual_rpm > 5000 && actual_rpm <= rpm5)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;
                panel7.Visible = true;
                panel8.Visible = true;
                panel9.Visible = true;
                panel10.Visible = true;
                panel11.Visible = true;
                panel12.Visible = true;
                panel13.Visible = true;
                panel14.Visible = true;
                panel15.Visible = true;
                panel16.Visible = true;
                panel17.Visible = true;

                //alsó korlát
                var rpmBottom = rpm4;
                //felső korlát
                var rpmTop = rpm5;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    panel18.Visible = true;
                    panel19.Visible = false;
                    panel20.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel18.Visible = true;
                    panel19.Visible = true;
                    panel20.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel18.Visible = true;
                    panel19.Visible = true;
                    panel20.Visible = true;
                }

                panel21.Visible = false;
                panel22.Visible = false;
                panel23.Visible = false;
            }
            else if (actual_rpm > 6000 && actual_rpm <= rpm6)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;
                panel7.Visible = true;
                panel8.Visible = true;
                panel9.Visible = true;
                panel10.Visible = true;
                panel11.Visible = true;
                panel12.Visible = true;
                panel13.Visible = true;
                panel14.Visible = true;
                panel15.Visible = true;
                panel16.Visible = true;
                panel17.Visible = true;
                panel18.Visible = true;
                panel19.Visible = true;
                panel20.Visible = true;

                //alsó korlát
                var rpmBottom = rpm5;
                //felső korlát
                var rpmTop = rpm6;

                //megmondom mennyi az rpm különbség amit 6 részre osztok
                var diff = rpmTop - rpmBottom;

                var rpmPercent = actual_rpm / (maxRpm / 100);

                //felosztás
                var oneThird = diff / 3;

                //
                var percent1 = (rpmBottom + oneThird) / (maxRpm / 100);
                var percent2 = (rpmBottom + oneThird * 2) / (maxRpm / 100);
                var percent3 = (rpmBottom + oneThird * 3) / (maxRpm / 100);

                //1
                if (rpmPercent < percent1 && rpmPercent >= rpmBottom)
                {
                    panel21.Visible = true;
                    panel22.Visible = false;
                    panel23.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    panel21.Visible = true;
                    panel22.Visible = true;
                    panel23.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    panel21.Visible = true;
                    panel22.Visible = true;
                    panel23.Visible = true;
                }
            }
            else if (actual_rpm > rpm6)
            {
                panel1.Visible = true;
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                panel6.Visible = true;
                panel7.Visible = true;
                panel8.Visible = true;
                panel9.Visible = true;
                panel10.Visible = true;
                panel11.Visible = true;
                panel12.Visible = true;
                panel13.Visible = true;
                panel14.Visible = true;
                panel15.Visible = true;
                panel16.Visible = true;
                panel17.Visible = true;
                panel18.Visible = true;
                panel19.Visible = true;
                panel20.Visible = true;
                panel21.Visible = true;
                panel22.Visible = true;
                panel23.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}