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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using MurphySecurity.Emails;

namespace MurphySecurity.Alerts
{
    public static partial class AlertManager
    {
        public abstract class Alert
        {
            private Stopwatch _stopwatch = new Stopwatch();
            private string _emailMessage = "";

            #region Public
            public Alert(string emailMessage)
            {
                _emailMessage = emailMessage;
                //Email repeater, sends an alert email every 3 mins for as long as the alert is on
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (AlertIsOn)
                            EmailsSender.SendEmailAlert("ALERT FROM MURPHY SECURITY ! " + DateTime.Now, _emailMessage);
                        Thread.Sleep(180000);
                    }
                });
            }

            public bool AlertIsOn { get; private set; }
            public double MinutesSinceLastAlert { get { return Math.Floor(_stopwatch.Elapsed.TotalMinutes); } }
            public double SecondsSinceLastAlert { get { return Math.Floor(_stopwatch.Elapsed.TotalSeconds); } }
            #endregion

            #region Virtual
            public virtual void LaunchAlert()
            {
                //Sends the core 433Mhz signal for enabled security
                //Only when on rpi
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Console.WriteLine(SendCode(AlarmCode).ReadToEnd());
                if (!AlertIsOn)
                {
                    _stopwatch.Restart();
                    EmailsSender.SendEmailAlert("ALERT FROM MURPHY SECURITY ! " + DateTime.Now, _emailMessage);
                }
                AlertIsOn = true;
            }

            public virtual void StopAlert() { AlertIsOn = false; }
            #endregion
        }
    }
}
