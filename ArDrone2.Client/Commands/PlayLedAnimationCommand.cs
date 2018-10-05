using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    

    public class PlayLedAnimationCommand : Command
    {
        private LedAnimation ledAnimation;
        private int frequency;
        private int duration;

        public PlayLedAnimationCommand(LedAnimation ledAnimation, int frequency, int duration)
        {
            this.ledAnimation = ledAnimation;
            this.frequency = frequency;
            this.duration = duration;
        }

        public override string ToString()
        {
            CheckSequenceNumber();
            return $"AT*LED={SequenceNumber},{(int) ledAnimation},{NumericExtensions.Normalize(frequency)},{duration}\r";
        }
    }
}
