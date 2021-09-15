﻿// MurphySecurity is a simple home security system designed to be used with Raspberry pi
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

using MurphySecurity.Alerts;

namespace MurphySecurity.Detectors
{
    public class KeyDetector : Detector
    {
        public long OpenCode { get; private set; }
        public long CloseCode { get; private set; }
        public long HomeCode { get; private set; }
        public long SOSCode { get; private set; }

        public KeyDetector(long openCode, long closeCode, long homeCode, long sosCode, string name)
        {
            OpenCode = openCode;
            CloseCode = closeCode;
            HomeCode = homeCode;
            SOSCode = sosCode;
            Name = name;
        }

        public override void UpdateStatus(long code)
        {
            //Always call the base UpdateStatus
            base.UpdateStatus(code);

            if (code == OpenCode)
                AlertManager.DisableSecurity();
            else if (code == CloseCode)
                AlertManager.EnableSecurity();
            else if (code == SOSCode)
                AlertManager.SOSAlert.LaunchAlert();
        }

        public override long[] GetCodes() { return new long[] { OpenCode, CloseCode, HomeCode, SOSCode }; }
    }
}
