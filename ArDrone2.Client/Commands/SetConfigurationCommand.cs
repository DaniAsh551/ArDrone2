using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class SetConfigurationCommand : Command
    {
        private String configurationKey;
        private String configurationValue;

        public SetConfigurationCommand(String configurationKey, String configurationValue)
            : base()
        {
            this.configurationKey = configurationKey;
            this.configurationValue = configurationValue;
        }

        public override string ToString()
        {
            CheckSequenceNumber();
            return $"AT*CONFIG={SequenceNumber},\"{configurationKey}\",\"{configurationValue}\"\r";
        }
    }
}
