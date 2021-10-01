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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using MurphySecurity.Detectors;
using MurphySecurity.Emails;
using MurphySecurity.Cameras;

namespace MurphySecurity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Starts the 433Mhz detector server
            DetectorManager.StartServer();
            //Starts the camera detection process
            Task.Factory.StartNew(() => {Camera.CameraDetectionClient();});
            //Triggers the EmailSender static constructor
            EmailsSender.Trigger();
            CreateHostBuilder(args).Build().Run();
            ///
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseStartup<Startup>();
                    //webBuilder.UseStartup<Startup>().UseUrls(urls: new String[] { "http://*:5000", "https://*:5001" });
                    webBuilder.UseStartup<Startup>().UseUrls(urls: new String[] { "http://*:80", "https://*:443" });
                });
    }
}
