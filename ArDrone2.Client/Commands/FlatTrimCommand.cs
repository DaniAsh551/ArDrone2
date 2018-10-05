using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class FlatTrimCommand : Command
    {
        public FlatTrimCommand()
            : base()
        { }

        public override string ToString()
        {
            CheckSequenceNumber();
            return string.Format("AT*FTRIM={0}\r", SequenceNumber);
        }
    }
}
