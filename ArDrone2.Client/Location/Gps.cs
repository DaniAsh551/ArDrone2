using System;
using System.Device.Location.GeoCoordinate;
using System.IO.Ports;
using System.Linq;
using System.Text;
using Ghostware.NMEAParser;
using Ghostware.NMEAParser.NMEAMessages;
using Ghostware.NMEAParser.NMEAMessages.Base;

namespace ArDrone2.Client.Location
{
    public class Gps
    {
        public GeoPosition<GeoCoordinate> Position { get; private set; }
        public readonly NmeaParser NmeaParser;
        public readonly string GpsDeviceName;
        public GpsState State;
        private SerialDevice GpsDevice;

        public Gps(string gpsDeviceName, BaudRate baudRate)
        {
            this.GpsDeviceName = gpsDeviceName;
            NmeaParser = new NmeaParser();
            this.State = new GpsState();
            
            GpsDevice = new SerialDevice(gpsDeviceName, baudRate);

            GpsDevice.DataReceived += (sender, bytes) =>
            {
                var nmeas = DeserializeMessages(bytes);
                
                nmeas.SetGpsState(ref State);
                
                var position = nmeas?.GetPosition();
                if (position == null)
                    return;
                Position = position;
                
                var args= new GpsUpdateEventArgs(Position, State, GpsDevice);
                OnGpsUpdate(this, args);
            };
        }

        public void Start()
        {
            GpsDevice.Open();
        }
        private NmeaMessage[] DeserializeMessages(byte[] bytes)
        {
            var rawText = Encoding.UTF8.GetString(bytes);
            var lines = rawText.Split('\n').Where(x => !x.StartsWith("$GNTXT")).ToArray();

            var nmeas = lines.Select(x => NmeaParser.Parse(x)).ToArray();
            return nmeas;
        }
        
        public event Action<Gps, GpsUpdateEventArgs> GpsUpdate;

        protected virtual void OnGpsUpdate(Gps gps, GpsUpdateEventArgs args)
        {
            GpsUpdate?.Invoke(gps, args);
        }
    }

    public class GpsUpdateEventArgs : EventArgs
    {
        public readonly GeoPosition<GeoCoordinate> Position;
        public readonly GpsState State;
        public readonly SerialDevice GpsDevice;

        public GpsUpdateEventArgs(GeoPosition<GeoCoordinate> position, GpsState state, SerialDevice gpsDevice)
        {
            this.Position = position;
            this.GpsDevice = gpsDevice;
            this.State = state;
        }
    }
}