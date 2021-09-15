# MurphySecurity is a simple home security system designed to be used with Raspberry pi
# Copyright (C) 2021 Jérémy LEPROU jerem.jlr@gmail.com
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY, without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

#!/usr/bin/python3
import rpi_rf
import time
import os
import socket
import pathlib

import logger

absolutePath = pathlib.Path(__file__).parent.absolute()
radioLogger = logger.getinitiedlogger(str(absolutePath)+"/Logs/RadioReceiverLogs/RadioReceiver.log")

#HOST = '192.168.1.60'  # Standard loopback interface address (localhost)
HOST = '127.0.0.1'
PORT = 14000   # Port to listen on (non-privileged ports are > 1023)

rfdevice = rpi_rf.RFDevice(24)
rfdevice.enable_rx()

while True:
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.connect((HOST, PORT))

            timestamp = None
            while True:
                if rfdevice.rx_code_timestamp != timestamp:
                    print(str(rfdevice.rx_code))
                    timestamp = rfdevice.rx_code_timestamp
                    s.send((str(rfdevice.rx_code)+"\n").encode("ascii"))
                time.sleep(0.01)
    except Exception as exception:
        radioLogger.critical(exception.args)


