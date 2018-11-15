using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ArDrone2.Client.Commands;
using System.Threading.Tasks;

namespace ArDrone2.Client
{
    public class DroneClient
    {
        public DroneClient(string droneIp, int clientPort = 5554, int senderPort = 5556)
        {
            Ip = droneIp;
            _clientPort = clientPort;
            _senderPort = senderPort;
            _client = new UdpClient(clientPort);
            _sender = new UdpClient(senderPort);
            DroneIpAddress = IPAddress.Parse(droneIp);
        }

       readonly UdpClient _client;
       readonly UdpClient _sender;

        public string Ip { get; set; }
        public IPAddress DroneIpAddress { get; set; }
        private readonly List<Command> _commands = new List<Command>();
        private readonly int _clientPort;
        private readonly int _senderPort;

        private uint LastSequenceNumber => _commands.Any() ? _commands.Last().SequenceNumber : 0;

        #region FlightMoveCommands
        //Up
        public bool GoUp(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, 0, 0, speed))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go up while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoUpWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoUp(speed);
            else
                Task.Factory.StartNew(() => GoUpWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go up until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoUpUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoUp(speed);
            else
                Task.Factory.StartNew(() => GoUpUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go up for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoUpFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoUpUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go up for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoUpFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoUpUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Down
        public bool GoDown(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, 0, 0, -1.0f * speed))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go down while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoDownWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoDown(speed);
            else
                Task.Factory.StartNew(() => GoDownWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go down until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoDownUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoDown(speed);
            else
                Task.Factory.StartNew(() => GoDownUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go down for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoDownFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoDownUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go down for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoDownFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoDownUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Forward
        public bool GoForward(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, -1.0f * speed, 0, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go forward while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoForwardWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoForward(speed);
            else
                Task.Factory.StartNew(() => GoForwardWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go forward until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoForwardUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoForward(speed);
            else
                Task.Factory.StartNew(() => GoForwardUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go forward for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoForwardFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoForwardUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go forward for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoForwardFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoForwardUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Backward
        public bool GoBack(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, speed, 0, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go back while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoBackWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoBack(speed);
            else
                Task.Factory.StartNew(() => GoBackWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go back until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoBackUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoBack(speed);
            else
                Task.Factory.StartNew(() => GoBackUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go back for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoBackFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoBackUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go back for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoBackFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoBackUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Left

        public bool GoLeft(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(-1.0f * speed, 0, 0, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go Left while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoLeftWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoLeft(speed);
            else
                Task.Factory.StartNew(() => GoLeftWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go Left until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoLeftUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoLeft(speed);
            else
                Task.Factory.StartNew(() => GoLeftUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go Left for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoLeftFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoLeftUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go Left for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoLeftFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoLeftUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        public bool GoRight(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(speed, 0, 0, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone go Right while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoRightWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoRight(speed);
            else
                Task.Factory.StartNew(() => GoRightWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go Right until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoRightUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoRight(speed);
            else
                Task.Factory.StartNew(() => GoRightUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone go Right for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoRightFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoRightUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone go Right for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoRightFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoRightUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Left

        public bool TurnLeft(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, 0, -1.0f * speed, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone turn Left while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoLeftWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoLeft(speed);
            else
                Task.Factory.StartNew(() => GoLeftWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone turn Left until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoLeftUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoLeft(speed);
            else
                Task.Factory.StartNew(() => GoLeftUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone turn Left for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoLeftFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoLeftUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone turn Left for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoLeftFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoLeftUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        //Turn Right
        public bool TurnRight(float speed = 1.0f)
            => (speed >= 0.1)
            ? SendCommand(new FlightMoveCommand(0, 0, speed, 0))
            : throw new ArgumentOutOfRangeException(nameof(speed));

        /// <summary>
        /// Makes the drone turn Right while the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="predicate"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoRightWhile(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (predicate(this))
                    GoRight(speed);
            else
                Task.Factory.StartNew(() => GoRightWhile(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone turn Right until the given predicate is true.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        public bool GoRightUntil(float speed = 1.0f, Func<DroneClient, bool> predicate, bool blockUntilDone = false)
        {
            if (blockUntilDone)
                while (!predicate(this))
                    GoRight(speed);
            else
                Task.Factory.StartNew(() => GoRightUntil(speed, predicate, false));
            return true;
        }

        /// <summary>
        /// Makes the drone turn Right for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="milliseconds"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoRightFor(float speed = 1.0f, int milliseconds, bool blockUntilDone = false)
        {
            var until = DateTime.Now.AddMilliseconds(milliseconds);
            return GoRightUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }

        /// <summary>
        /// Makes the drone turn Right for a specified length in time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="timeSpan"></param>
        /// <param name="blockUntilDone">Whether the drone thread must be blocked until the execution finishes.</param>
        /// <returns></returns>
        public bool GoRightFor(float speed = 1.0f, TimeSpan timeSpan, bool blockUntilDone = false)
        {
            var until = DateTime.Now.Add(timeSpan);
            return GoRightUntil(speed, x => DateTime.Now >= until, blockUntilDone);
        }


        public bool Stop()
            => SendCommand(new FlightMoveCommand(0, 0, 0, 0));
        #endregion


        public bool SendCommand(Command command)
        {
            command.SequenceNumber = LastSequenceNumber + 1;
            var bytes = command.GetBytes();
            var ret = _sender.Send(bytes, bytes.Length, new IPEndPoint(DroneIpAddress, _senderPort));
            return ret > -1;
        }
        
    }
}