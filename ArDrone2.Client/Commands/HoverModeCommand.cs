using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class HoverModeCommand : Command
    {
        private HoverMode mode;

        public HoverModeCommand(HoverMode mode)
            : base()
        {
            this.mode = mode;
        }

        public override string ToString()
        {
            CheckSequenceNumber();
            return String.Format("AT*PCMD={0},{1},{2},{3},{4},{5}\r", SequenceNumber, (mode == HoverMode.Hover) ? 0 : 1, 0, 0, 0, 0);
        }
    }
}
