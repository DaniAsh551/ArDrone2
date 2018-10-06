using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArDrone2.Client.NavData
{
    public class NavDataRetriever
    {
        private readonly UdpClient client;
        private IPEndPoint endpoint;
        
        private const int keepAliveSignalInterval = 200;
        private Stopwatch keepAliveStopwatch = new Stopwatch();
        
        private const int initialSequenceNumber = 0;

        private uint checksum;
        private NavigationDataHeaderStruct currentNavigationDataHeaderStruct;
        private NavigationDataStruct currentNavigationDataStruct;

        private DroneData currentNavigationData;

        private uint currentSequenceNumber;

        private bool initialized = false;
        private bool commandModeEnabled = false;

        public NavDataRetriever(string remoteIpAddress, int port, int timeoutValue, UdpClient client = null)
        {
            ResetVariables();
            keepAliveStopwatch = new Stopwatch();
            endpoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), port);
            this.client = client ?? new UdpClient(5554);
        }

        protected void ResetVariables()
        {
            keepAliveStopwatch.Reset();
            
            currentNavigationDataStruct = new NavigationDataStruct();
            currentNavigationDataHeaderStruct = new NavigationDataHeaderStruct();

            currentNavigationData = new DroneData();

            currentSequenceNumber = initialSequenceNumber;
        }

        public void WaitForFirstMessageToArrive()
        {
            int currentRetries = 0;
            int maxRetries = 20;

            while (currentRetries < maxRetries &&
                   currentNavigationDataHeaderStruct.Status == 0)
            {
                currentRetries++;
                Thread.Sleep(50);
            }
        }

        protected virtual void StartKeepAliveSignal()
        {
            keepAliveStopwatch.Restart();
        }

        protected bool IsKeepAliveSignalNeeded()
        {
            if (keepAliveStopwatch.ElapsedMilliseconds > keepAliveSignalInterval)
            {
                keepAliveStopwatch.Restart();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Action<int> SendMessage;

        public void Start() => Task.Factory.StartNew(ProcessWorkerThread);
        
        protected void  ProcessWorkerThread()
        {
            SendMessage(1);
            StartKeepAliveSignal();

            do
            {
                if (IsKeepAliveSignalNeeded())
                    SendMessage(1);

                byte[] buffer = ReceiveData();

                if (buffer != null)
                {
                    DetermineNavigationDataHeader(buffer);
                    if (IsNavigationDataHeaderValid())
                    {
                        UpdateNavigationData(buffer);

                        if (!IsChecksumValid(buffer))
                            ProcessInvalidChecksum();
                    }

                    currentSequenceNumber = currentNavigationDataHeaderStruct.SequenceNumber;
                }
            }
            while (!false);
        }

        private byte[] ReceiveData()
        {
            byte[] buffer = null;
            try
            {
                if (client != null)
                    buffer = client.Receive(ref endpoint);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10060) //Timeout
                    SendMessage(1);

                if (client != null)
                    buffer = client.Receive(ref endpoint);
            }

            return buffer;
        }

        protected void AfterDisconnect()
        {
            ResetVariables();
        }

        public void ResetSequenceNumber()
        {
            currentSequenceNumber = initialSequenceNumber;
        }

        private bool IsNavigationDataHeaderValid()
        {
            return currentNavigationDataHeaderStruct.Header == 0x55667788;
        }

        private void UpdateNavigationData(byte[] buffer)
        {
            MemoryStream memoryStream;
            BinaryReader reader;
            InitializeBinaryReader(buffer, out memoryStream, out reader);

            while (memoryStream.Position < memoryStream.Length)
            {
                ushort tag = reader.ReadUInt16();
                ushort size = reader.ReadUInt16();

                if (IsNavigationData(tag))
                {
                    DetermineNavigationData(buffer, (int)(memoryStream.Position - 4));
                    memoryStream.Position += size - 4;
                }
                else if (IsNavigationDataCheckSum(tag))
                {
                    checksum = reader.ReadUInt32();
                }
                else
                {
                    memoryStream.Position += size - 4;
                }
            }
        }

        private void InitializeBinaryReader(byte[] buffer, out MemoryStream memoryStream, out BinaryReader reader)
        {
            memoryStream = new MemoryStream(buffer);
            reader = new BinaryReader(memoryStream);

            memoryStream.Position = Marshal.SizeOf(typeof(NavigationDataHeaderStruct));
        }

        private bool IsNavigationData(ushort tag)
        {
            return tag == 0;
        }

        private bool IsNavigationDataCheckSum(ushort tag)
        {
            return tag == 0xFFFF;
        }

        private bool IsChecksumValid(byte[] buffer)
        {
            return CalculateChecksum(buffer) == checksum;
        }

        private uint CalculateChecksum(byte[] buffer)
        {
            uint checksum = 0;  
            for (uint index = 0; index < buffer.Length - 8; index++)
            {
                checksum += buffer[index];
            }

            return checksum;
        }

        private void ProcessInvalidChecksum()
        {
            // TODO implement
        }

        private void DetermineNavigationDataHeader(byte[] buffer)
        {
            unsafe
            {
                fixed (byte* entry = &buffer[0])
                {
                    currentNavigationDataHeaderStruct = *(NavigationDataHeaderStruct*)entry;
                }
            }

            SetStatusFlags(currentNavigationDataHeaderStruct.Status);
            //Console.WriteLine(currentNavigationDataHeaderStruct.Status);
        }

        private void DetermineNavigationData(byte[] buffer, int position)
        {
            unsafe
            {
                fixed (byte* entry = &buffer[position])
                {
                    currentNavigationDataStruct = *(NavigationDataStruct*)entry;
                }
            }

            currentNavigationData = new DroneData(currentNavigationDataStruct);
        }

        private void SetStatusFlags(uint state)
        {
            uint initializedState = state & 2048;      // 11th bit of the status entry
            uint commandModeState = state & 64;       // 8th bit of the status entry

            initialized = initializedState == 0;
            commandModeEnabled = commandModeState != 0;
        }

        public DroneData CurrentNavigationData
        {
            get
            {
                return currentNavigationData;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }

        public bool IsCommandModeEnabled
        {
            get
            {
                return commandModeEnabled;
            }
        }
    }
}