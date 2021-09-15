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
import threading
import camera

#opencv_thread = threading.Thread(target=camera.opencv_processing)
#opencv_thread.start()

camera.load_config()
camera.save_config()

image_thread = threading.Thread(target=camera.image_udpserver)
image_thread.start()

handshake_thread = threading.Thread(target=camera.handshake_udpserver)
handshake_thread.start()

alert_thread = threading.Thread(target=camera.alert_server)
alert_thread.start()

data_thread = threading.Thread(target=camera.data_server)
data_thread.start()

