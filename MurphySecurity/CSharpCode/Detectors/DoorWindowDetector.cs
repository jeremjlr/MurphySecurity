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

using MurphySecurity.Alerts;

namespace MurphySecurity.Detectors
{
    // All the non static stuff
    public class DoorWindowDetector : Detector
    {
        public long OpenCode { get; private set; }
        public long CloseCode { get; private set; }
        public bool IsOpen { get; private set; }
        public DoorWindowDetector(long openCode, long closeCode, string name)
        {
            OpenCode = openCode;
            CloseCode = closeCode;
            Name = name;
        }

        public override void UpdateStatus(long code)
        {
            //Always call the base UpdateStatus
            base.UpdateStatus(code);
            if (code == OpenCode)
            {
                IsOpen = true;
                AlertManager.IntrusionAlert.LaunchAlert();
            }
            else if (code == CloseCode)
                IsOpen = false;
        }

        public override long[] GetCodes() { return new long[] { OpenCode, CloseCode }; }
    }
}
