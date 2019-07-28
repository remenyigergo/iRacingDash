using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRacingSdkWrapper;
using iRacingSdkWrapper.Broadcast;
using Newtonsoft.Json;

namespace MockSdkWrapper
{
    public class MockSdkWrapper : ISdkWrapper
    {
        private List<ITelemetryInfo> _telemetryInfos = new List<ITelemetryInfo>();
        private int _index = 0;
        private readonly SynchronizationContext _context;

        public MockSdkWrapper()
        {
            _context = SynchronizationContext.Current;

            var dataSource = File.ReadAllLines("C:\\data.txt");
            foreach (var line in dataSource)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new TelemetryValueConverter(typeof(MockTelemetryValue<>)));

                var telemetryInfo = JsonConvert.DeserializeObject<MockTelemetryInfo>(line, settings);
                _telemetryInfos.Add(telemetryInfo);

                //RaiseEvent(OnTelemetryUpdated, new SdkWrapper.TelemetryUpdatedEventArgs(telemetryInfo, DateTime.Now.Ticks));

                //Thread.Sleep(33);
            }
        }

        public global::iRSDKSharp.iRacingSDK Sdk => throw new NotImplementedException();

        public SdkWrapper.EventRaiseTypes EventRaiseType { get; set; }

        private bool _IsRunning;
        public bool IsRunning => _IsRunning;

        private bool _IsConnected;
        public bool IsConnected => _IsConnected;

        public double TelemetryUpdateFrequency { get; set; }
        public int ConnectSleepTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int DriverId => throw new NotImplementedException();

        public ReplayControl Replay => throw new NotImplementedException();

        public CameraControl Camera => throw new NotImplementedException();

        public PitCommandControl PitCommands => throw new NotImplementedException();

        public ChatControl Chat => throw new NotImplementedException();

        public TextureControl Textures => throw new NotImplementedException();

        public TelemetryRecordingControl TelemetryRecording => throw new NotImplementedException();

        public event EventHandler<SdkWrapper.TelemetryUpdatedEventArgs> TelemetryUpdated;
        public event EventHandler<SdkWrapper.SessionInfoUpdatedEventArgs> SessionInfoUpdated;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public object GetData(string headerName)
        {
            throw new NotImplementedException();
        }

        public ITelemetryValue<T> GetTelemetryValue<T>(string name)
        {
            throw new NotImplementedException();
        }

        public void RequestSessionInfoUpdate()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            _IsRunning = true;
            _IsConnected = true;            

            var period = Helpers.FpsCalculator.GetPeriodFromFPS(this.TelemetryUpdateFrequency);
             //var timer = new Timer(MainLoop, null, 0, period);

            //RaiseEvent(OnTelemetryUpdated, new SdkWrapper.TelemetryUpdatedEventArgs(_telemetryInfos[_index++], DateTime.Now.Ticks));
            Do();

            //OnTelemetryUpdated(new SdkWrapper.TelemetryUpdatedEventArgs(new MockTelemetryInfo(), DateTime.Now.Ticks));
            //OnTelemetryUpdated(new SdkWrapper.TelemetryUpdatedEventArgs(new MockTelemetryInfo(), DateTime.Now.Ticks));
            //OnSessionInfoUpdated(new SdkWrapper.SessionInfoUpdatedEventArgs("", DateTime.Now.Ticks));
        }

        private async Task Do()
        {
            var sw = Stopwatch.StartNew();
            Dictionary<int, int> log = new Dictionary<int, int>();            

            for (_index = 0; _index<_telemetryInfos.Count; _index++)
            {                
                RaiseEvent(OnTelemetryUpdated, new SdkWrapper.TelemetryUpdatedEventArgs(_telemetryInfos[_index], DateTime.Now.Ticks));

                if (log.ContainsKey(DateTime.Now.Second)) log[DateTime.Now.Second]++;
                else log.Add(DateTime.Now.Second, 0);

                await Task.Delay(27);
            }
            sw.Stop();
        }

        public void Stop()
        {
            _IsRunning = false;
            _IsConnected = false;
        }

        private void MainLoop(object state)
        {
            RaiseEvent(OnTelemetryUpdated, new SdkWrapper.TelemetryUpdatedEventArgs(_telemetryInfos[_index++], DateTime.Now.Ticks));
        }

        private void RaiseEvent<T>(Action<T> del, T e)
            where T : EventArgs
        {
            var callback = new SendOrPostCallback(obj => del(obj as T));

            if (_context != null && this.EventRaiseType == SdkWrapper.EventRaiseTypes.CurrentThread)
            {
                // Post the event method on the thread context, this raises the event on the thread on which the SdkWrapper object was created
                _context.Post(callback, e);
            }
            else
            {
                // Simply invoke the method, this raises the event on the background thread that the SdkWrapper created
                // Care must be taken by the user to avoid cross-thread operations
                callback.Invoke(e);
            }
        }

        private void OnTelemetryUpdated(SdkWrapper.TelemetryUpdatedEventArgs e)
        {            
            var handler = this.TelemetryUpdated;
            if (handler != null) handler(this, e);
        }

        private void OnSessionInfoUpdated(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            var handler = this.SessionInfoUpdated;
            if (handler != null) handler(this, e);
        }
    }
}
