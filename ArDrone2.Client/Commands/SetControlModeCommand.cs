using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class SetControlModeCommand : Command
    {
        private DroneControlMode mode;

        public SetControlModeCommand(DroneControlMode mode)
            : base()
        {
            this.mode = mode;
        }

        public override string ToString()
        {
            CheckSequenceNumber();
            return $"AT*CTRL={SequenceNumber},{(int) mode},0\r";
        }
    }
}
