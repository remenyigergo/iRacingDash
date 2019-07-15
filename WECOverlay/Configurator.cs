using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WECOverlay
{
    class Configurator
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


    }
}
