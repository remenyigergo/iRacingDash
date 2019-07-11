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

namespace WECOverlay
{
    public partial class Form1 : Form
    {
        private SdkWrapper wrapper;

        private int driverCarIdx;
        private int carNumber;
        private string teamName;
        private float actual_rpm;
        private float maxRpm = 7300;
        private string carClassColor;
        private int sessionNumber;
        private int carPosition;

        int rpm1 = 2000;
        int rpm2 = 3000;
        int rpm3 = 4000;
        int rpm4 = 5000;
        int rpm5 = 6000;
        int rpm6 = 7000;

        public Form1()
        {
            InitializeComponent();

            Init();

            wrapper = new SdkWrapper();
            wrapper.Start();
            wrapper.TelemetryUpdateFrequency = 30;
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

            throttle.Size = new Size(12, 33);
            brake.Size = new Size(18, 39);

        }

        private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            CalculateGear(e);
            var speedinKmh = (int)(e.TelemetryInfo.Speed.Value * 3.6);
            speed_value.Text = speedinKmh.ToString();
            sessionNumber = e.TelemetryInfo.SessionNum.Value;
            position.Text = e.TelemetryInfo.CarIdxClassPosition.Value[driverCarIdx].ToString();

            SetClassColor(e);
            ShiftLights(e);
            Pedals(e);
        }

        private void SetClassColor(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            switch (carClassColor)
            {
                case "0xffda59":
                    class_picture.Image = Image.FromFile(@"C:\Users\Greg\source\repos\iRacingDash\git\iRacingDash\WECOverlay\images\class_ffda59.png");
                    break;
                case "0x33ceff":
                    class_picture.Image = Image.FromFile(@"C:\Users\Greg\source\repos\iRacingDash\git\iRacingDash\WECOverlay\images\class_33ceff.png");
                    break;
                case "0xff5888":
                    class_picture.Image = Image.FromFile(@"C:\Users\Greg\source\repos\iRacingDash\git\iRacingDash\WECOverlay\images\class_ff5888.png");
                    break;
                default:
                    class_picture.Image = Image.FromFile(@"C:\Users\Greg\source\repos\iRacingDash\git\iRacingDash\WECOverlay\images\class_default.png");
                    break;
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

            throttle.Size = new Size(12 + (int)(throttleUnit * widthThrottle), 33);
            brake.Size = new Size(18 + (int)(brakeUnit * widthBrake), 39);

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
                driverCarIdx = Int32.Parse(e.SessionInfo["DriverInfo"]["DriverCarIdx"].Value);
                carNumber = Int32.Parse(e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["CarNumber"]
                    .Value);
                teamName = e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["TeamName"].Value;
                carClassColor = e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", driverCarIdx]["CarClassColor"].Value;
                var telemetryDiskFile = e.SessionInfo["WeekendInfo"]["TelemetryOptions"]["TelemetryDiskFile"].Value;

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
            } else if (actual_rpm > rpm6)
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
