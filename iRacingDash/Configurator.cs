using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iRacingDash
{
    class Configurator
    {
        public float Configurate(string descendant, string element, string attribute)
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
                        return float.Parse(elem.Attribute(attribute).Value);
                    }
                    
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }

    }
}
