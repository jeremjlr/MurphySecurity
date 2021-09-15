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

namespace MurphySecurity.Cameras
{
    public class CameraSettings : ICloneable
    {
        public string Id { get; set; }
        /// <summary>
        /// Returns true if detection and alert triggering is enabled on this camera
        /// </summary>
        public bool DetectionEnabled { get; set; }
        public float ImageCompressor { get; set; }
        public int NoiseReducer { get; set; }
        public int DetectionSensibility { get; set; }
        public int DetectionTolerance { get; set; }

        public object Clone()
        {
            CameraSettings clone = new CameraSettings();
            clone.Id = Id;
            clone.DetectionEnabled = DetectionEnabled;
            clone.ImageCompressor = ImageCompressor;
            clone.NoiseReducer = NoiseReducer;
            clone.DetectionSensibility = DetectionSensibility;
            clone.DetectionTolerance = DetectionTolerance;
            return clone;
        }
    }
}
