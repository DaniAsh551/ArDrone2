using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using ArDrone2.Client.NavData;

namespace ArDrone2.Console
{
    class Program
    {

        static void Main(string[] args)
        {
            var client = new UdpClient(5554);
            var sender = new UdpClient(5556);

            var ip = "192.168.100.55";
            var exitBs = Encoding.ASCII.GetBytes("AT*CONFIG=\"general:navdata_demo\",\"TRUE\"\r");
            var ack = Encoding.ASCII.GetBytes("AT*CTRL=0\r");
            sender.Send(exitBs, exitBs.Length, ip, 5556);
            sender.Send(ack, ack.Length, ip, 5556);
            client.Send(new byte[] {1}, 1, ip, 5554);
            
            var nav = new NavDataRetriever("192.168.100.55", 5554, 2000, client);
            nav.SendMessage = i => client.Send(new byte[] {(byte)i}, 1, ip, 5554);
            nav.Start();
            nav.WaitForFirstMessageToArrive();
            var navData = nav.CurrentNavigationData;
            while (true)
            {
                navData = nav.CurrentNavigationData;
            }
            
        }
        
        static void Main2(string[] args)
        {
            var client = new UdpClient(5554);
            var sender = new UdpClient(5556);

            var ip = "192.168.100.55";
            var exitBs = Encoding.ASCII.GetBytes("AT*CONFIG=\"general:navdata_demo\",\"TRUE\"\r");
            var ack = Encoding.ASCII.GetBytes("AT*CTRL=0\r");
            sender.Send(exitBs, exitBs.Length, ip, 5556);
            sender.Send(ack, ack.Length, ip, 5556);
            client.Send(new byte[] {1}, 1, ip, 5554);
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), 5554);
            var data = client.Receive(ref endPoint);
            var str = Encoding.ASCII.GetString(data);
        }
    }
}