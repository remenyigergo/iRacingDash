using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ValueConverter(typeof(TelemetryValue<>)));
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var ti = new TelemetryInfo() {Speed = new TelemetryValue<float>() {Value = 100}};
            var ser = JsonConvert.SerializeObject(ti, settings);

            Console.WriteLine(ser);

            var des = JsonConvert.DeserializeObject<TelemetryInfo>(ser, settings);
            Console.WriteLine(des);
        }
        
    }
    
    public interface ITelemetryValue<T>
    {
        T Value { get; set; }
    }

    public interface ITelemetryInfo
    {
        ITelemetryValue<float> Speed { get; set; }
    }

    public class TelemetryInfo : ITelemetryInfo
    {
        public TelemetryInfo()
        {
            
        }


        public TelemetryInfo(double i)
        {
            
        }

        public ITelemetryValue<float> Speed { get; set; }
    }

    public class TelemetryValue<T> : ITelemetryValue<T>
    {
        public TelemetryValue()
        {
            
        }

        public TelemetryValue(T value)
        {
            Value = value;
        }

        public TelemetryValue(double i)
        {
            
        }

        public T Value { get; set; }
    }
}
