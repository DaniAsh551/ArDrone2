using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class FlightMoveCommand : Command
    {
        private float roll;
        private float pitch;
        private float yaw;
        private float gaz;

        public FlightMoveCommand(float roll, float pitch, float yaw, float gaz)
        {
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
            this.gaz = gaz;
        }
        public override string ToString()
        {
            CheckSequenceNumber();
            return
                $"AT*PCMD={SequenceNumber},{1},{roll.Normalize()},{pitch.Normalize()},{gaz.Normalize()},{yaw.Normalize()}\r";
        }

        public float Roll => roll;

        public float Pitch => pitch;

        public float Yaw => yaw;

        public float Gaz => gaz;
    }
}
