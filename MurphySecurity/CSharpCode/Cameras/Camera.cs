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
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MurphySecurity.Alerts;

namespace MurphySecurity.Cameras
{
    public class Camera
    {
        private bool _closed = false;
        //Will be sent to the cam
        //Says that the cam needs to save the config locally
        private bool _needToSaveConfig = false;
        private string _recordPath = "";
        //Locker for the _needToSaveConfig multhreading
        private readonly object _locker = new object();

        private static ConcurrentDictionary<string, Camera> _cameraDictionary = new ConcurrentDictionary<string, Camera>();
        //To make the recording process on multiple cams thread safe ( ie: not create multiple folders for the same alert)
        private static readonly object _staticLocker = new object();
        private static string _recordDirectory = "";

        #region Private
        private Camera(string ip, string id)
        {
            Ip = ip;
            Settings = new CameraSettings()
            {
                Id = id
            };
            Task.Factory.StartNew(() => { UpdateData(ip); });
            Task.Factory.StartNew(() => { UpdateImageClient(ip); });
            Task.Factory.StartNew(() => { UpdateAlert(ip); });
        }

        private static IPAddress GetLocalIp()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                socket.Close();
                return endPoint.Address;
            }
        }

        private static void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            //Asks if the detected device is a camera
            //Adds it to the dictionnary if it's not already in it
            string ip = e.UserState.ToString();
            if (e.Reply != null && e.Reply.Status == IPStatus.Success && !_cameraDictionary.ContainsKey(ip))
            {
                try
                {
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    using UdpClient testConnection = new UdpClient(ip, 13000);
                    testConnection.Client.ReceiveTimeout = 500;
                    byte[] askIfCamera = Encoding.ASCII.GetBytes("areyoucamera");
                    testConnection.Send(askIfCamera, askIfCamera.Length);
                    byte[] answerBytes = testConnection.Receive(ref serverEndPoint);
                    string answer = Encoding.ASCII.GetString(answerBytes);
                    Thread.Sleep(5000);
                    Camera camera = new Camera(ip, answer);
                    if (!_cameraDictionary.TryAdd(ip, camera))
                        camera.Close(ip);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Udp handshake error : {exception.Message}");
                }
            }
        }
        /// <summary>
        /// A udp client that asks the camera for image updates
        /// </summary>
        private void UpdateImageClient(string ip)
        {
            int port = 13001;
            try
            {
                using UdpClient imageClient = new UdpClient(ip, port);
                imageClient.Client.ReceiveTimeout = 5000;
                IPEndPoint imageEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //Sent to tell the server we're core client
                byte[] iAmCoreBytes = Encoding.ASCII.GetBytes("iamcore");
                imageClient.Send(iAmCoreBytes, iAmCoreBytes.Length);
                while (!_closed)
                {
                    byte[] receivedBytes = imageClient.Receive(ref imageEndPoint);
                    string base64 = Convert.ToBase64String(receivedBytes);
                    ImageSrc = string.Format("data:jpeg;base64,{0}", base64);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR IMAGE CLIENT: " + exception.Message);
                Close(ip);
            }
        }

        /// <summary>
        /// A client entirely dedicated to getting alerts from cameras
        /// </summary>
        /// <param name="ip"></param>
        private void UpdateAlert(string ip)
        {
            int port = 13002;
            try
            {
                using UdpClient alertClient = new UdpClient(ip, port);
                alertClient.Client.ReceiveTimeout = 5000;
                IPEndPoint alertEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //Sent to tell the server we're core client
                byte[] iAmCoreBytes = Encoding.ASCII.GetBytes("iamcore");
                alertClient.Send(iAmCoreBytes, iAmCoreBytes.Length);
                while (!_closed)
                {
                    byte[] receivedBytes = alertClient.Receive(ref alertEndPoint);
                    string alertStatus = Encoding.ASCII.GetString(receivedBytes);
                    Console.WriteLine("ALERT STATUS : " + alertStatus);
                    string[] stringSplit = alertStatus.Split(":", StringSplitOptions.RemoveEmptyEntries);
                    int[] alertTimeData = new int[stringSplit.Length];
                    for (int i = 0; i < stringSplit.Length; i++)
                        alertTimeData[i] = int.Parse(stringSplit[i]);
                    DateTime alertTime = new DateTime(alertTimeData[0], alertTimeData[1], alertTimeData[2], alertTimeData[3], alertTimeData[4], alertTimeData[5]);
                    //If equal to min value then we haven't received any alert time yet
                    //Which means we can't know if it is an actual alert or not
                    if (LastDetectionTime.CompareTo(DateTime.MinValue) != 0 && LastDetectionTime.CompareTo(alertTime) != 0)
                        AlertManager.IntrusionAlert.LaunchAlert();
                    LastDetectionTime = alertTime;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR ALERT CLIENT: " + exception.Message);
                Close(ip);
            }
        }

        /// <summary>
        /// A client used to receive and send all the data to the cameras
        /// </summary>
        /// <param name="ip"></param>
        private void UpdateData(string ip)
        {
            int port = 13003;
            try
            {
                using UdpClient dataClient = new UdpClient(ip, port);
                dataClient.Client.ReceiveTimeout = 5000;
                IPEndPoint configEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //Sent to tell the server we're core client
                byte[] iAmCoreBytes = Encoding.ASCII.GetBytes("iamcore");
                dataClient.Send(iAmCoreBytes, iAmCoreBytes.Length);

                byte[] dataBytes = dataClient.Receive(ref configEndPoint);
                string dataStatus = Encoding.ASCII.GetString(dataBytes);

                string[] readInfo = dataStatus.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                //We don't receive the id here because we already received it during the camera image handshake
                Settings.DetectionEnabled = bool.Parse(readInfo[0]);
                Settings.ImageCompressor = float.Parse(readInfo[1], CultureInfo.InvariantCulture.NumberFormat);
                Settings.NoiseReducer = int.Parse(readInfo[2]);
                Settings.DetectionSensibility = int.Parse(readInfo[3]);
                Settings.DetectionTolerance = int.Parse(readInfo[4]);
                Settings.FrameTolerance = int.Parse(readInfo[5]);

                while (!_closed)
                {
                    bool record = AlertManager.IntrusionAlert.AlertIsOn || AlertManager.FireAlert.AlertIsOn || AlertManager.SOSAlert.AlertIsOn;
                    DateTime time = DateTime.Now;
                    //Locks to make sure we do not create multiple directories for the same alert
                    lock (_staticLocker)
                    {
                        if (record && !Recording)
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                DirectoryInfo dInfo = Directory.CreateDirectory(AlertManager.ALERTS_PATH + "/" + "AlertOnCore(" + time.Year + "-" + time.Month + "-" + time.Day + "_" + time.Hour + "-" + time.Minute + "-" + time.Second + ")");
                                _recordDirectory = dInfo.Name + "/";
                            }
                            else
                                _recordDirectory = "";
                        }
                        Recording = record;
                    }
                    _recordPath = _recordDirectory + Settings.Id + "(" + Ip + ")" + time.Year + "-" + time.Month + "-" + time.Day + "_" + time.Hour + "-" + time.Minute + "-" + time.Second;

                    //Locks to make sure config won't be changed while we send it
                    lock (_locker)
                    {
                        string imageCompressorString = Settings.ImageCompressor.ToString("F1", new CultureInfo("en-US").NumberFormat);
                        dataStatus = Settings.Id + "\n" + Settings.DetectionEnabled + "\n" + imageCompressorString + "\n" + Settings.NoiseReducer + "\n" + Settings.DetectionSensibility + "\n" + Settings.DetectionTolerance + "\n" + Settings.FrameTolerance + "\n" + record + "\n" + _recordPath + "\n" + _needToSaveConfig;
                        dataBytes = Encoding.ASCII.GetBytes(dataStatus);
                        dataClient.Send(dataBytes, dataBytes.Length);
                        if (_needToSaveConfig)
                            _needToSaveConfig = false;
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR DATA : " + exception.Message);
                Close(ip);
            }
        }

        private void Close(string ip)
        {
            Camera camera;
            _closed = true;
            //Waiting for all connections to timeout and all threads to end
            Thread.Sleep(10000);
            _cameraDictionary.TryRemove(ip, out camera);

        }
        #endregion

        #region Public
        /// <summary>
        /// Returns true if the cameras are currently recording
        /// </summary>
        public static bool Recording { get; private set; }
        /// <summary>
        /// The settings of the camera.
        /// </summary>
        public CameraSettings Settings { get; set; }
        /// <summary>
        /// The camera image stream as a string, that is updated every frame recieved from the camera.
        /// </summary>
        public string ImageSrc { get; private set; }
        public DateTime LastDetectionTime { get; private set; }
        public string Ip { get; }

        /// <summary>
        /// The camera will be asked to save the config locally.
        /// </summary>
        public void SaveConfig()
        {
            //Locks to make sure config won't be changed while we send it
            lock (_locker)
                _needToSaveConfig = true;
        }

        public static string[] GetAllCamerasIP()
        {
            return _cameraDictionary.Keys.ToArray();
        }

        public static bool TryGetCamera(string ip, out Camera camera)
        {
            return _cameraDictionary.TryGetValue(ip, out camera);
        }

        /// <summary>
        /// A client that continuously attempts to detect cameras on the same network
        /// </summary>
        public static void CameraDetectionClient()
        {
            Thread.Sleep(5000);
            while (true)
            {
                try
                {
                    while (true)
                    {
                        IPAddress localIp = GetLocalIp();
                        //x.x.x.255
                        string localIpToSearch = localIp.GetAddressBytes()[0] + "." + localIp.GetAddressBytes()[1] + "." + localIp.GetAddressBytes()[2] + ".";
                        //Searches everything between x.x.x.0 and x.x.x.254
                        for (int i = 0; i < 255; i++)
                        {
                            Ping p = new Ping();
                            p.PingCompleted += PingCompleted;
                            string ipToCheck = localIpToSearch + i;
                            p.SendAsync(ipToCheck, 250, ipToCheck);
                        }
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR IN CAMERA SEARCH DETECTION : " + e.Message);
                }
                Thread.Sleep(10000);
            }
        }
        #endregion
    }
}
