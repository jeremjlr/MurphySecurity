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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MurphySecurity.Alerts
{
    public static partial class AlertManager
    {
        public const string ALERTS_PATH = "wwwroot/Alerts";
        private const string CODES_PATH = "wwwroot/Data/codes.txt";
        private const string PASSWORD_PATH = "wwwroot/Data/password.txt";
        private const string RF_TRANSMITTER_GPIO = "26";
        private static readonly object _locker = new object();

        #region Private
        static AlertManager()
        {
            LoadCodes();
            LoadPassword();
        }

        private static void LoadCodes()
        {
            try
            {
                string[] readInfo = File.ReadAllText(CODES_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                OpenCode = long.Parse(readInfo[0]);
                CloseCode = long.Parse(readInfo[1]);
                AlarmCode = long.Parse(readInfo[2]);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error loading codes : " + exception.Message);
            }
        }

        private static void LoadPassword()
        {
            try
            {
                string[] readInfo = File.ReadAllText(PASSWORD_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Password = readInfo[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error loading password : " + exception.Message);
            }
        }

        private static void SaveCodes()
        {
            lock (_locker)
            {
                try
                {
                    string toWrite = OpenCode + "\n" + CloseCode + "\n" + AlarmCode;
                    File.WriteAllText(CODES_PATH, toWrite);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error saving codes : " + exception.Message);
                }
            }
        }

        private static void SavePassword()
        {
            lock (_locker)
            {
                try
                {
                    string toWrite = Password;
                    File.WriteAllText(PASSWORD_PATH, toWrite);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error saving password : " + exception.Message);
                }
            }
        }

        //Sends the given code via the rf 433Mhz transmitter
        private static StreamReader SendCode(long code)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "rpi-rf_send",
                Arguments = "-g " + RF_TRANSMITTER_GPIO + " " + code.ToString()
            };
            Process process = Process.Start(startInfo);
            return process.StandardOutput;
        }
        #endregion

        #region Public
        /// <summary>
        /// If true, the system will be monitoring and launching alerts
        /// </summary>
        public static bool SecurityIsActive { get; private set; }
        public static long OpenCode { get; private set; }
        public static long CloseCode { get; private set; }
        public static long AlarmCode { get; private set; }
        public static string Password { get; private set; }

        public static void ChangePassword(string password)
        {
            Password = password;
            SavePassword();
        }

        public static void GenerateNewCodes()
        {
            //Formula for the codes is
            //openCode = (rand*8)+2
            //closeCode = (rand*8)+1
            //alarmCode = (rand*8)+8
            //max number for codes is around 165000+code ex:16445601/16445602/16445608
            lock (_locker)
            {
                Random rand = new Random();
                int randomValue = rand.Next(1000, 2062500);
                OpenCode = (randomValue * 8) + 2;
                CloseCode = (randomValue * 8) + 1;
                AlarmCode = (randomValue * 8) + 8;
            }
            SaveCodes();
        }

        public static void EnableSecurity()
        {
            //Sends the core 433Mhz signal for enabled security
            //Only when on rpi
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine(SendCode(CloseCode).ReadToEnd());
            SecurityIsActive = true;
        }

        public static void DisableSecurity()
        {
            //Sends the core 433Mhz signal for enabled security
            //Only when on rpi
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine(SendCode(OpenCode).ReadToEnd());
            SecurityIsActive = false;
            IntrusionAlert.StopAlert();
            FireAlert.StopAlert();
            SOSAlert.StopAlert();
        }

        /// <summary>
        /// Emits the OpenCode in order to setup the core with an external alarm
        /// </summary>
        public static void EmitCode()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine(SendCode(CloseCode).ReadToEnd());
        }

        public class IntrusionAlertClass : Alert
        {
            public IntrusionAlertClass(string emailMessage) : base(emailMessage) { }
            public override void LaunchAlert()
            {
                //No alert launched if the security is turned off
                if (SecurityIsActive)
                    base.LaunchAlert();
            }
        }
        public static IntrusionAlertClass IntrusionAlert = new IntrusionAlertClass("AN INTRUSION WAS DETECTED !\nPlease go check it.");
        public class FireAlertClass : Alert
        {
            public FireAlertClass(string emailMessage) : base(emailMessage) { }
        }
        public static FireAlertClass FireAlert = new FireAlertClass("A FIRE WAS DETECTED !\nPlease go check it.");

        public class SOSClass : Alert
        {
            public SOSClass(string emailMessage) : base(emailMessage) { }
        }
        public static SOSClass SOSAlert = new SOSClass("SOMEONE JUST LAUNCHED A SOS!\nPlease go check it.");
        #endregion
    }
}
