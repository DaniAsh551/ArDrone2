using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    
    public abstract class Command
    {
        public uint SequenceNumber { get; set; }

        protected Command()
        {
        }

        protected void CheckSequenceNumber()
        {
            if (SequenceNumber == 0)
                throw new InvalidOperationException("The command must be sequenced before it can be sent");
        }

        public abstract override string ToString();
        public byte[] GetBytes()
            => Encoding.UTF8.GetBytes(ToString());
    }
}
