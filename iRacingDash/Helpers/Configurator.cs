using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace iRacingDash
{
    public class Configurator
    {
        public T Configurate<T>(string descendant, string element, string attribute)
        {
            string startupPath = Environment.CurrentDirectory;
            
            var initConfig= XDocument.Load(startupPath+"\\externalConfig.config")
                .Descendants("init");

            var Config = initConfig.Descendants(descendant).FirstOrDefault();

            if (Config != null)
            {
                foreach (var elem in Config.Elements(element))
                {
                    var elementAttribute = elem.Attribute(attribute);
                    if (elementAttribute != null)
                    {
                        return (T)Convert.ChangeType(elem.Attribute(attribute).Value.ToString(), typeof(T));
                    }
                    
                }
                return (T)Convert.ChangeType(0, typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(-1, typeof(T));
            }
        }


        public void StartConfig(ref float maxRpm, ref Point location, ref float minRpmPercent, ref float shiftLight1Percent, ref float shiftLight2Percent, ref float redLinePercent, ref int telemetryUpdateFrequency)
        {
            //CONFIGS
            //Car RPM

            maxRpm = Configurate<int>("car", "config", "MaxRpm");

            //Window setup
            var X = Configurate<int>("window", "config", "PositionX");
            var Y = Configurate<int>("window", "config", "PositionY");
            location = new Point((int)X, (int)Y);

            //ShiftLights setup
            minRpmPercent = Configurate<int>("led", "config", "MinimumRPMPercent");
            shiftLight1Percent = Configurate<int>("led", "config", "ShiftLightGreenPercent");
            shiftLight2Percent = Configurate<int>("led", "config", "ShiftLightYellowPercent");
            redLinePercent = Configurate<int>("led", "config", "ShiftLightRedPercent");

            telemetryUpdateFrequency = Configurate<int>("fps", "config", "TelemetryFps");
        }

    }

}
