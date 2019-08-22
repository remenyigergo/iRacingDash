using iRacingDash.Helpers;
using iRacingDash.Sessions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using iRacingSdkWrapper;

namespace iRacingDash
{
    public class Dash
    {
        private Form1 _dashForm;
        private PictureBox _idleImg;
        private FormManipulator _manipulator;
        private delegate void SafeCallDelegate(bool visible, string time);
        private SdkWrapper _wrapper;

        //Panels
        public Panel settingsPanel;
        public Label settingLabelTitle;
        public Label settingLabelValue;
        public Label idleClock;

        //LEDS
        public float minRpmPercent;
        public float shiftLight1Percent;
        public float shiftLight2Percent;
        public float redLinePercent;
        public float maxRpm = 3700;

        public int settingPanelFps = 2;
        public int settingPanelFpsCounter = -1;

        public Dash(Form1 form, SdkWrapper wrapper)
        {
            _dashForm = form;
            
            _wrapper = wrapper;
            _idleImg = new PictureBox();
            _manipulator = new FormManipulator(this._dashForm);
        }

        public void InitDash()
        {
            _dashForm.Last_lap_title.ForeColor = Color.Gold;
            _dashForm.led1_1.Visible = false;
            _dashForm.led1_2.Visible = false;
            _dashForm.led1_3.Visible = false;
            _dashForm.led1_4.Visible = false;
            _dashForm.led1_5.Visible = false;
            _dashForm.led1_6.Visible = false;
            _dashForm.led2_1.Visible = false;
            _dashForm.led2_2.Visible = false;
            _dashForm.led2_3.Visible = false;
            _dashForm.led3_1.Visible = false;
            _dashForm.led3_2.Visible = false;
            _dashForm.led3_3.Visible = false;
            _dashForm.Laptime_value.ForeColor = Color.LawnGreen;

            //Create labels and panels
            settingLabelTitle = _manipulator.CreateLabel("settingLabelTitle", "Setting", new Size(_dashForm.Width, 150), new Point(0, 0),
                Color.Black, Color.DarkGray, new Font("Microsoft YaHei", 40, FontStyle.Bold), false);
            settingLabelValue = _manipulator.CreateLabel("settingLabelValue", "Value", new Size(_dashForm.Width, 150), new Point(0, 150),
                Color.Black, Color.DarkGray, new Font("Microsoft YaHei", 40, FontStyle.Bold), false);
            settingLabelTitle.TextAlign = ContentAlignment.BottomCenter;
            settingLabelValue.TextAlign = ContentAlignment.TopCenter;

            settingsPanel = _manipulator.CreateSettingWindow(new Point(0, 0), Color.DarkGray, new Size(_dashForm.Width, _dashForm.Height),
                false);


            CreateIdle();


            //panelek előtérbe helyezése
            settingLabelTitle.BringToFront();
            settingLabelValue.BringToFront();
            //settingsPanel.BringToFront();


            Thread t = new Thread(new ThreadStart(CheckWrapperRunning));
            t.Start();
        }


        private void CreateIdle()
        {
            Image image = iRacingDash.Properties.Resources.idle;
            this._idleImg = new PictureBox
            {
                Name = "idleImg",
                Size = new Size(370, 300),
                Location = new Point(0, 0),
                Image = image,
            };
            _dashForm.Controls.Add(_idleImg);
            _idleImg.MouseDown += new MouseEventHandler(_dashForm.Idle_MouseDown);
            _idleImg.MouseUp += new MouseEventHandler(_dashForm.Idle_MouseUp);
            _idleImg.MouseMove += new MouseEventHandler(_dashForm.Idle_MouseMove);
            //_idleImg.DoubleClick += new EventHandler(_dashForm.Idle_DoubleClick);

            _idleImg.Visible = true;


            this.idleClock = _manipulator.CreateLabel("idleClock", "00:00", new Size(130, 50), new Point(120, 30), Color.White, Color.Transparent,
                new Font("Microsoft YaHei", 30, FontStyle.Regular), true);
            idleClock.Parent = _idleImg;

            _idleImg.BringToFront();
            idleClock.BringToFront();
        }


        private void WriteTextSafe(bool visible, string time)
        {
            if (_idleImg.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                _dashForm.Invoke(d, new object[] { visible, time });
            }
            else
            {
                _idleImg.Visible = visible;
                idleClock.Text = time;
            }

            if (idleClock.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                _dashForm.Invoke(d, new object[] { visible, time });
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
                var isConnected = _wrapper.IsConnected;

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

        public void UpdateLapTimeV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var lapObject = _wrapper.GetData("LapCurrentLapTime");
            var lap = Convert.ToDouble(lapObject);

            int min = (int)(lap / 60);
            double sec = (lap % 60);
            string laptime;

            laptime = string.Format("{0:00}:{1:00.000}", min, sec);
            _dashForm.Laptime_value.Text = laptime;
        }

        public void CalculateDeltaV2(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var deltaObject = _wrapper.GetData("LapDeltaToSessionBestLap");
            var deltaInt = Convert.ToDouble(deltaObject);

            if (deltaInt > 0 && deltaInt < 99.99)
            {
                _dashForm.delta_panel.BackColor = Color.Firebrick;
                _dashForm.Delta_value.Text = string.Format("{0:+0.00}", deltaInt);
            }
            else if (deltaInt < 0 && deltaInt > -99.99)
            {
                _dashForm.delta_panel.BackColor = Color.Green;
                _dashForm.Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt == 0)
            {
                _dashForm.delta_panel.BackColor = Color.FromArgb(255, 40, 40, 40);
                _dashForm.Delta_value.Text = string.Format("{0:0.00}", deltaInt);
            }
            else if (deltaInt >= 99.99)
            {
                _dashForm.delta_panel.BackColor = Color.Firebrick;
                _dashForm.Delta_value.Text = "+99.99";
            }
            else if (deltaInt <= -99.99)
            {
                _dashForm.delta_panel.BackColor = Color.Green;
                _dashForm.Delta_value.Text = "-99.99";
            }
        }

        public void CarSettingPanelFlash<T>(SdkWrapper.TelemetryUpdatedEventArgs e, ref T actualValue, ref T valueTemp,
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
                    if (settingPanelFpsCounter <= (int)(_wrapper.TelemetryUpdateFrequency / settingPanelFps))
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

        public void CarLeftRight(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var carLeftRight = _wrapper.GetData("CarLeftRight").ToString();


            switch (Int32.Parse(carLeftRight))
            {
                case 2:
                    _dashForm.carLeftPanel.Visible = true;
                    break;
                case 3:
                    _dashForm.carRightPanel.Visible = true;
                    break;
                case 5:
                    _dashForm.carLeftPanel.Visible = true;
                    _dashForm.carRightPanel.Visible = true;
                    break;
                default:
                    _dashForm.carLeftPanel.Visible = false;
                    _dashForm.carRightPanel.Visible = false;
                    break;
            }

        }

        public void UpdateRpmLights(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            var rpm = e.TelemetryInfo.RPM.Value;

            var rpmPercent = rpm / (maxRpm / 100);

            //ha zöld előtt járunk minden led off
            if (rpmPercent < minRpmPercent)
            {
                //zöldek
                _dashForm.led1_1.Visible = false;
                _dashForm.led1_2.Visible = false;
                _dashForm.led1_3.Visible = false;
                _dashForm.led1_4.Visible = false;
                _dashForm.led1_5.Visible = false;
                _dashForm.led1_6.Visible = false;

                //sárga
                _dashForm.led2_1.Visible = false;
                _dashForm.led2_2.Visible = false;
                _dashForm.led2_3.Visible = false;

                //piros
                _dashForm.led3_1.Visible = false;
                _dashForm.led3_2.Visible = false;
                _dashForm.led3_3.Visible = false;
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
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = false;
                    _dashForm.led1_3.Visible = false;
                    _dashForm.led1_4.Visible = false;
                    _dashForm.led1_5.Visible = false;
                    _dashForm.led1_6.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = true;
                    _dashForm.led1_3.Visible = false;
                    _dashForm.led1_4.Visible = false;
                    _dashForm.led1_5.Visible = false;
                    _dashForm.led1_6.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = true;
                    _dashForm.led1_3.Visible = true;
                    _dashForm.led1_4.Visible = false;
                    _dashForm.led1_5.Visible = false;
                    _dashForm.led1_6.Visible = false;
                }
                else if (rpmPercent > percent3 && rpmPercent < percent4)
                {
                    // 4   
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = true;
                    _dashForm.led1_3.Visible = true;
                    _dashForm.led1_4.Visible = true;
                    _dashForm.led1_5.Visible = false;
                    _dashForm.led1_6.Visible = false;
                }
                else if (rpmPercent > percent4 && rpmPercent < percent5)
                {
                    // 5
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = true;
                    _dashForm.led1_3.Visible = true;
                    _dashForm.led1_4.Visible = true;
                    _dashForm.led1_5.Visible = true;
                    _dashForm.led1_6.Visible = false;
                }
                else if (rpmPercent > percent5 && rpmPercent < minRpmPercent + shiftLight1Percent)
                {
                    // 6
                    _dashForm.led1_1.Visible = true;
                    _dashForm.led1_2.Visible = true;
                    _dashForm.led1_3.Visible = true;
                    _dashForm.led1_4.Visible = true;
                    _dashForm.led1_5.Visible = true;
                    _dashForm.led1_6.Visible = true;
                }

                _dashForm.led2_1.Visible = false;
                _dashForm.led2_2.Visible = false;
                _dashForm.led2_3.Visible = false;

                _dashForm.led3_1.Visible = false;
                _dashForm.led3_2.Visible = false;
                _dashForm.led3_3.Visible = false;


                //gear changes to white
                _dashForm.gear.ForeColor = Color.Black;

                _dashForm.gear_panel.BackColor = Color.Gold;
                _dashForm.gear.ForeColor = Color.Black;
            }


            if (rpmPercent > minRpmPercent + shiftLight1Percent &&
                rpmPercent <= minRpmPercent + shiftLight1Percent + shiftLight2Percent)
            {
                //ha a sárgában vagyunk

                //alapból az összest kivilágitjuk a zöldekből
                _dashForm.led1_1.Visible = true;
                _dashForm.led1_2.Visible = true;
                _dashForm.led1_3.Visible = true;
                _dashForm.led1_4.Visible = true;
                _dashForm.led1_5.Visible = true;
                _dashForm.led1_6.Visible = true;


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
                    _dashForm.led2_1.Visible = true;
                    _dashForm.led2_2.Visible = false;
                    _dashForm.led2_3.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    _dashForm.led2_1.Visible = true;
                    _dashForm.led2_2.Visible = true;
                    _dashForm.led2_3.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    _dashForm.led2_1.Visible = true;
                    _dashForm.led2_2.Visible = true;
                    _dashForm.led2_3.Visible = true;
                }

                _dashForm.led3_1.Visible = false;
                _dashForm.led3_2.Visible = false;
                _dashForm.led3_3.Visible = false;

                //gear changes to white
                _dashForm.gear.ForeColor = Color.White;

                _dashForm.gear_panel.BackColor = Color.Gold;
                _dashForm.gear.ForeColor = Color.Black;
            }


            if (rpmPercent > redLinePercent)
            {
                //ha a pirosban vagyunk

                //alapból az összest kivilágitjuk zöldből és sárgából
                _dashForm.led1_1.Visible = true;
                _dashForm.led1_2.Visible = true;
                _dashForm.led1_3.Visible = true;
                _dashForm.led1_4.Visible = true;
                _dashForm.led1_5.Visible = true;
                _dashForm.led1_6.Visible = true;
                _dashForm.led2_1.Visible = true;
                _dashForm.led2_2.Visible = true;
                _dashForm.led2_3.Visible = true;

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
                    _dashForm.led3_1.Visible = true;
                    _dashForm.led3_2.Visible = false;
                    _dashForm.led3_3.Visible = false;
                }
                else if (rpmPercent > percent1 && rpmPercent < percent2)
                {
                    // 2
                    _dashForm.led3_1.Visible = true;
                    _dashForm.led3_2.Visible = true;
                    _dashForm.led3_3.Visible = false;
                }
                else if (rpmPercent > percent2 && rpmPercent < percent3)
                {
                    // 3
                    _dashForm.led3_1.Visible = true;
                    _dashForm.led3_2.Visible = true;
                    _dashForm.led3_3.Visible = true;
                }

                //gear changes to red
                _dashForm.gear.ForeColor = Color.Red;
                _dashForm.gear_panel.BackColor = Color.Orange;
            }

            if (rpmPercent > redLinePercent)
            {
                //ha már villogni kell
                //TODO

                _dashForm.gear_panel.BackColor = Color.Red;
                _dashForm.gear.ForeColor = Color.White;
            }
        }

    }
}