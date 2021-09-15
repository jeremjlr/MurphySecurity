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
import logging
from logging.handlers import RotatingFileHandler

def getinitiedlogger(path: str):
    log_formatter = logging.Formatter('%(asctime)s %(levelname)s %(funcName)s(%(lineno)d) %(message)s')
    log_file = path
    log_handler = RotatingFileHandler(log_file, mode='a', maxBytes=5*1024*1024, backupCount=1, encoding=None, delay=0)
    log_handler.setFormatter(log_formatter)
    log_handler.setLevel(logging.INFO)
    app_log = logging.getLogger('root')
    app_log.setLevel(logging.INFO)
    app_log.addHandler(log_handler)
    return app_log
