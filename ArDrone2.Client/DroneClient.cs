using System;
using System.Collections.Generic;
using System.Linq;
using ArDrone2.Client.Commands;

namespace ArDrone2.Client
{
    public class DroneClient
    {
        public string Ip { get; set; }
        
        private readonly List<Command> _commands = new List<Command>();
        private uint LastSequenceNumber => _commands.Any() ? _commands.Last().SequenceNumber : 0;
        
        
    }
}