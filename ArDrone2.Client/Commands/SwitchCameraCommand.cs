using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArDrone2.Client.Commands
{
    public class SwitchCameraCommand : Command
    {
        private readonly CameraMode _cameraMode;

        public SwitchCameraCommand(CameraMode cameraMode)
            : base()
        {
            this._cameraMode = cameraMode;
        }

        public override string ToString()
        {
            CheckSequenceNumber();

            return $"AT*CONFIG={SequenceNumber},\"{"video:video_channel"}\",\"{(int) _cameraMode}\"\r";
        }

        public CameraMode CameraMode => _cameraMode;
    }
}
