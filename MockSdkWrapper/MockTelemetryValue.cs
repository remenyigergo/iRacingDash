using iRacingSdkWrapper;
using iRSDKSharp;

namespace MockSdkWrapper
{
    public class MockTelemetryValue<T> : ITelemetryValue<T>
    {
        public MockTelemetryValue(T v)
        {
            Value = v;
        }

        public T Value { get; set; }

        public bool Exists { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public CVarHeader.VarType Type { get; set; }

        public object GetValue()
        {
            return Value;
        }
    }
}