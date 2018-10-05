using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    /// <summary>
    /// This command disables the system control watchdog, allowing controls
    /// </summary>
    public class WatchDogCommand : Command
    {
        public WatchDogCommand()
            : base()
        { }

        public override string ToString()
        {
            CheckSequenceNumber();
            return $"AT*COMWDG={SequenceNumber}\r";
        }
    }
}
