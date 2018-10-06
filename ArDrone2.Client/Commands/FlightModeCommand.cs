using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class FlightModeCommand : Command
    {
        private readonly DroneFlightMode _flightMode;

        public FlightModeCommand(DroneFlightMode flightMode)
            : base()
        {
            this._flightMode = flightMode;
        }

        public override string ToString()
        {
            CheckSequenceNumber();
            return $"AT*REF={SequenceNumber},{(int) _flightMode}\r";
        }
    }
}