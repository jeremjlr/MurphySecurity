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

namespace MurphySecurity.Detectors
{
    public abstract class Detector
    {
        //Stopwatch to measure minutes since last information
        private Stopwatch _stopwatch = new Stopwatch();
        public string Name { get; set; }
        /// <summary>
        /// Returns true if the detector has received information at least once.
        /// </summary>
        public bool HasInformation { get; private set; }
        public double MinutesSinceLastInformation { get { return Math.Floor(_stopwatch.Elapsed.TotalMinutes); } }

        public virtual void UpdateStatus(long code)
        {
            HasInformation = true;
            _stopwatch.Restart();
        }

        /// <summary>
        /// Returns an array of all the codes of that detector
        /// </summary>
        public abstract long[] GetCodes();
    }
}
