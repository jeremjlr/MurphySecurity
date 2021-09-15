// MurphySecurity is a simple home security system designed to be used with Raspberry pi
// Copyright (C) 2021 Jérémy LEPROU jerem.jlr@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MurphySecurity
{
    /// <summary>
    /// Not fully implemented class/functionality
    /// Has no use for now
    /// </summary>
    public static class WifiInfoServer
    {
        private static readonly object locker = new object();
        private static bool _started = false;
        private const string WIFIINFO_PATH = "wwwroot/Data/wifiinfo.txt";
        public static string[] WifiInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Saves the wifi info in a .txt file
        /// </summary>
        public static void SaveWifiInfo()
        {
            //To avoid possible conflict if 2 updates happen at the same time
            lock (locker)
            {
                try
                {
                    if (WifiInfo[0].Equals(""))
                    {
                        File.WriteAllText(WIFIINFO_PATH, $"None\n{WifiInfo[1]}");
                        WifiInfoServer.WifiInfo[0] = "None";
                    }
                    else
                    {
                        File.WriteAllText(WIFIINFO_PATH, $"{WifiInfo[0]}\n{WifiInfo[1]}");
                    }
                }
                catch (Exception exception)
                {
                    //Do nothing
                }
            }
        }

        public static void StartServer()
        {
            if (!_started)
            {
                _started = true;
                WifiInfo = new string[] { "", "" };
                try
                {
                    //WifiInfo must always have a length of 2
                    string[] readInfo = File.ReadAllText(WIFIINFO_PATH).Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    WifiInfo[0] = readInfo[0];
                    WifiInfo[1] = readInfo[1];
                }
                catch (Exception exception)
                {
                    //Do nothing
                }
                Task.Factory.StartNew(() => { UdpWifiInfoServer(); });
            }
        }
        
        private static void UdpWifiInfoServer()
        {
            while (true)
            {
                try
                {
                    using UdpClient server = new UdpClient(20000);
                    IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);

                    byte[] recievedBytes = server.Receive(ref client);
                    string recievedData = Encoding.ASCII.GetString(recievedBytes);
                    Console.WriteLine("Wifi server recieved from : " + client.Address);
                    if (recievedData.Equals("givewifi"))
                    {
                        byte[] answerBytes = Encoding.ASCII.GetBytes(WifiInfo[0] + "\n" + WifiInfo[1]);
                        server.Send(answerBytes, answerBytes.Length, client);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in wifi server :" + exception.Message);
                }
            }
        }

        //For emulating
        private static bool _newCore = true;
        /// <summary>
        /// This is just for testing and emulating a camera
        /// </summary>
        public static void UdpServer()
        {
            byte[] imageBytes = File.ReadAllBytes("wwwroot/Images/jeanclaude.jpg");
            while (true)
            {
                try
                {
                    using UdpClient client = new UdpClient(13001);
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = client.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receivedBytes);
                    Console.WriteLine("Image server received : " + returnData);
                    if (returnData.Equals("iamcore"))
                    {
                        _newCore = false;
                    }
                    while (!_newCore)
                    {
                        client.Send(imageBytes, imageBytes.Length, RemoteIpEndPoint);
                        Thread.Sleep(33);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in image server : " + exception.Message);
                }
            }
        }

        /// <summary>
        /// This is just for testing and emulating a camera
        /// </summary>
        public static void UdpHandShakeServer()
        {
            UdpClient server = new UdpClient(13000);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    byte[] receivedBytes = server.Receive(ref RemoteIpEndPoint);
                    string receivedData = Encoding.ASCII.GetString(receivedBytes);
                    Console.WriteLine("Handshake server recieved : " + receivedData);
                    if (receivedData.Equals("areyoucamera"))
                    {
                        Console.WriteLine("Handshake server answering to port : " + RemoteIpEndPoint.Port);
                        byte[] answeredBytes = Encoding.ASCII.GetBytes("iamcamera");
                        server.Send(answeredBytes, answeredBytes.Length, RemoteIpEndPoint);
                        _newCore = true;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in handshake server : " + exception.Message);
                }
            }
        }
    }
}
