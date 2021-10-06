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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

using MurphySecurity.Alerts;

namespace MurphySecurity.Detectors
{
    // All the static stuff
    public static class DetectorManager
    {
        private const string DETECTORS_PATH = "wwwroot/Data/detectors.txt";
        private static bool _started = false;
        private static ConcurrentDictionary<long, Detector> _detectorDictionnary = new ConcurrentDictionary<long, Detector>();
        //A list of all the potential detectors 
        //Each List<long> represents a potential detector which can have up to 4 codes
        //Lock used for _waitingDetectors 
        private static object _locker = new object();
        private static List<List<long>> _waitingDetectors = new List<List<long>>();

        #region Private
        private static void UpdateDetectors(long code, bool updateWaitingDetectors)
        {
            Detector detector;
            //If the code is not already used by a detector we add it to the _waitingDetectors
            //Else we use it to update the detectors
            if (!_detectorDictionnary.TryGetValue(code, out detector))
            {
                if (updateWaitingDetectors)
                {
                    bool codeAdded = false;
                    lock (_locker)
                    {
                        foreach (List<long> waitingDetector in _waitingDetectors)
                        {
                            if (waitingDetector.Contains(code))
                            {
                                codeAdded = true;
                                break;
                            }
                            else
                                //This can cause some detectors to conflict with each other
                                //The chance is pretty low though
                                for (int i = -12; i <= 12; i++)
                                {
                                    if (waitingDetector.Contains(code + i))
                                    {
                                        waitingDetector.Add(code);
                                        codeAdded = true;
                                        break;
                                    }
                                }
                        }
                        if (!codeAdded)
                        {
                            List<long> newDetector = new List<long>();
                            newDetector.Add(code);
                            _waitingDetectors.Add(newDetector);
                        }
                    }
                }
            }
            else
                detector.UpdateStatus(code);
        }

        /// <summary>
        /// Server in charge of receiving all the 433MHZ codes from the radioreceiver.py python script
        /// </summary>
        private static void Tcp433MHZDetectorServer()
        {
            while (true)
            {
                int port = 14000;
                IPAddress localAddr = IPAddress.Parse("0.0.0.0");
                TcpListener server = new TcpListener(localAddr, port);
                try
                {
                    server.Start();
                    while (true)
                    {
                        // Buffer for reading data
                        Byte[] bytes = new Byte[256];
                        //Waiting for connection
                        TcpClient newClient = server.AcceptTcpClient();
                        //Create a new task that will use that client
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                using TcpClient client = newClient;
                                NetworkStream stream = client.GetStream();
                                int numberOfBytes;
                                long lastCode = 0;
                                while ((numberOfBytes = stream.Read(bytes, 0, bytes.Length)) != 0)
                                {
                                    string data = Encoding.ASCII.GetString(bytes, 0, numberOfBytes);
                                    string[] codes = data.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
                                    //Last value will be empty so we ignore it
                                    for (int i = 0; i < codes.Length - 1; i++)
                                    {
                                        long code = long.Parse(codes[i]);
                                        //Only updates waiting detectors if we get 2 identical codes in a row, so the system won't think noise codes are new detectors 
                                        //A lot of noise codes seem to be lower than 1000 (or even 100) so add this to avoid most of them
                                        //Normally no detectors should use information codes lower than 1000 so it shouldn't cause any issue
                                        if (code > 1000)
                                        {
                                            if (code == lastCode)
                                            {
                                                //If the code detected was not emitted by the core itself
                                                bool test = code != AlertManager.OpenCode && code != AlertManager.CloseCode && code != AlertManager.AlarmCode;
                                                if (code != AlertManager.OpenCode && code != AlertManager.CloseCode && code != AlertManager.AlarmCode)
                                                    UpdateDetectors(long.Parse(codes[i]), true);
                                            }
                                            else
                                                UpdateDetectors(long.Parse(codes[i]), false);
                                            lastCode = code;
                                        }
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine("ERROR in TCP Detector server : " + exception.Message);
                            }
                        });
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("ERROR IN THE 433MHZ SERVER : " + exception.Message);
                }
                finally
                {
                    server.Stop();
                }
            }
        }

        private static void LoadDetectors()
        {
            try
            {
                string[] readInfo = File.ReadAllText(DETECTORS_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                while (i < readInfo.Length)
                {
                    if (readInfo[i] == "DoorWindowDetector")
                    {
                        DoorWindowDetector door_window_detector = new DoorWindowDetector(long.Parse(readInfo[i + 1]), long.Parse(readInfo[i + 2]), readInfo[i + 3]);
                        TryAddDetector(door_window_detector);
                        i = i + 4;
                    }
                    else if (readInfo[i] == "MovementDetector")
                    {
                        MovementDetector movement_detector = new MovementDetector(long.Parse(readInfo[i + 1]), readInfo[i + 2]);
                        TryAddDetector(movement_detector);
                        i = i + 3;
                    }
                    else if (readInfo[i] == "KeyDetector")
                    {
                        KeyDetector key_detector = new KeyDetector(long.Parse(readInfo[i + 1]), long.Parse(readInfo[i + 2]), long.Parse(readInfo[i + 3]), long.Parse(readInfo[i + 4]), readInfo[i + 5]);
                        TryAddDetector(key_detector);
                        i = i + 6;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        #endregion

        #region Public
        public static void StartServer()
        {
            if (!_started)
            {
                _started = true;
                LoadDetectors();
                Task.Factory.StartNew(() => { Tcp433MHZDetectorServer(); });
            }
        }

        public static HashSet<Detector> GetDetectors()
        {
            return new HashSet<Detector>(_detectorDictionnary.Values);
        }

        /// <summary>
        /// Returns a list of waiting detectors as arrays of codes (long)
        /// </summary>
        /// <returns></returns>
        public static List<long[]> GetWaitingDetectors()
        {
            //We do this to avoid object users to modify _waitingDetectors and to avoid any conccurency issues
            List<long[]> toReturn = new List<long[]>();
            //Turn it into array to avoid conccurency issues while manipulating values;
            lock (_locker)
            {
                List<long>[] waitingDetectorsArray = _waitingDetectors.ToArray();
                foreach (List<long> waitingDetector in waitingDetectorsArray)
                {
                    toReturn.Add(waitingDetector.ToArray());
                }
            }
            return toReturn;
        }

        public static void TryRemoveDetector(Detector detector)
        {
            if (detector is DoorWindowDetector)
            {
                DoorWindowDetector door_window_detector = (DoorWindowDetector)detector;
                _detectorDictionnary.TryRemove(door_window_detector.CloseCode, out detector);
                _detectorDictionnary.TryRemove(door_window_detector.OpenCode, out detector);
            }
            else if (detector is MovementDetector)
            {
                MovementDetector movement_detector = (MovementDetector)detector;
                _detectorDictionnary.TryRemove(movement_detector.MovementCode, out detector);
            }
            else if (detector is KeyDetector)
            {
                KeyDetector key_detector = (KeyDetector)detector;
                _detectorDictionnary.TryRemove(key_detector.OpenCode, out detector);
                _detectorDictionnary.TryRemove(key_detector.CloseCode, out detector);
                _detectorDictionnary.TryRemove(key_detector.HomeCode, out detector);
                _detectorDictionnary.TryRemove(key_detector.SOSCode, out detector);
            }
            //Always save after removing a detector
            SaveDetectors();
        }

        public static void SaveDetectors()
        {
            string toSave = "";
            lock (_locker)
            {
                foreach (Detector detector in GetDetectors())
                {
                    if (detector is DoorWindowDetector)
                    {
                        DoorWindowDetector door_window_detector = (DoorWindowDetector)detector;
                        string toAdd = "DoorWindowDetector" + "\n" + door_window_detector.OpenCode + "\n" + door_window_detector.CloseCode + "\n" + door_window_detector.Name + "\n";
                        toSave = toSave + toAdd;
                    }
                    else if (detector is MovementDetector)
                    {
                        MovementDetector movement_detector = (MovementDetector)detector;
                        string toAdd = "MovementDetector" + "\n" + movement_detector.MovementCode + "\n" + movement_detector.Name + "\n";
                        toSave = toSave + toAdd;
                    }
                    else if (detector is KeyDetector)
                    {
                        KeyDetector key_detector = (KeyDetector)detector;
                        string toAdd = "KeyDetector" + "\n" + key_detector.OpenCode + "\n" + key_detector.CloseCode + "\n" + key_detector.HomeCode + "\n" + key_detector.SOSCode + "\n" + key_detector.Name + "\n";
                        toSave = toSave + toAdd;
                    }
                }

                try
                {
                    File.WriteAllText(DETECTORS_PATH, toSave);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("ERROR IN SAVE DETECTOR : " + exception.Message);
                }
            }
        }

        /// <summary>
        /// Adds the detector to the detectors list and removes it from the waiting detectors
        /// </summary>
        /// <param name="detector"></param>
        public static void TryAddDetector(Detector detector)
        {
            if (detector is DoorWindowDetector)
            {
                DoorWindowDetector door_window_detector = (DoorWindowDetector)detector;
                _detectorDictionnary.TryAdd(door_window_detector.OpenCode, door_window_detector);
                _detectorDictionnary.TryAdd(door_window_detector.CloseCode, door_window_detector);
                //Removes the detector from the waiting detectors
                RemoveWaitingDetector(new long[] { door_window_detector.OpenCode, door_window_detector.CloseCode });
            }
            else if (detector is MovementDetector)
            {
                MovementDetector movement_detector = (MovementDetector)detector;
                _detectorDictionnary.TryAdd(movement_detector.MovementCode, movement_detector);
                RemoveWaitingDetector(new long[] { movement_detector.MovementCode });
            }
            else if (detector is KeyDetector)
            {
                KeyDetector key_detector = (KeyDetector)detector;
                _detectorDictionnary.TryAdd(key_detector.OpenCode, key_detector);
                _detectorDictionnary.TryAdd(key_detector.CloseCode, key_detector);
                _detectorDictionnary.TryAdd(key_detector.HomeCode, key_detector);
                _detectorDictionnary.TryAdd(key_detector.SOSCode, key_detector);
                RemoveWaitingDetector(new long[] { key_detector.OpenCode, key_detector.CloseCode, key_detector.HomeCode, key_detector.SOSCode });
            }
            else if (detector is SmokeDetector)
            {
                SmokeDetector smoke_detector = (SmokeDetector)detector;
                _detectorDictionnary.TryAdd(smoke_detector.SmokeCode, smoke_detector);
                RemoveWaitingDetector(new long[] { smoke_detector.SmokeCode });
            }
            //Always save after adding a new detector
            SaveDetectors();
        }

        public static void RemoveAllWaitingDetectors()
        {
            lock (_locker)
            {
                _waitingDetectors.Clear();
            }
        }

        public static void RemoveWaitingDetector(long[] waitingDetectorToRemove)
        {
            lock (_locker)
            {
                List<long> toRemove = new List<long>();
                foreach (List<long> waitingDetector in _waitingDetectors)
                    for (int i = 0; i < waitingDetectorToRemove.Length; i++)
                        if (waitingDetector.Contains(waitingDetectorToRemove[i]))
                            toRemove = waitingDetector;
                _waitingDetectors.Remove(toRemove);
            }
        }
        #endregion
    }
}
