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
import os
import socket
import sys
import threading
import time
#from typing import Any
#import picamera
#import picamera.array
import cv2 as cv
import pathlib

import logger

coreIP = None   
newCore = True
record = False
recordPath = ""
alertTime = "0001:1:1:00:00:00"
CONFIG_PATH = "config.txt"
absolutePath = pathlib.Path(__file__).parent.absolute()
cameraLogger = logger.getinitiedlogger(str(absolutePath)+"/Logs/CameraLogs/CameraImageServer.log")

#Config attributes with default values
camera_id = "number0"
detection_enabled = False
new_image_compressor = 0.5
image_compressor = 0.5
noise_reducer = 5
detection_sensibility = 20
detection_tolerance = 1

baseline_image = None
base_time = time.time()

def handshake_udpserver():
    # 0.0.0.0 because this is a server
    localAddress = ("0.0.0.0", 13000)
    bufferSize  = 1024
    while True:
        try:
            # Create a datagram socket
            with socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM) as udpServerSocket:
                global newCore
                # Bind to address and ip
                udpServerSocket.bind(localAddress)
                print("Handshake UDP server up and listening")
                # Listen for incoming datagrams
                bytesAddressPair = udpServerSocket.recvfrom(bufferSize)
                message = bytesAddressPair[0].decode("ascii")
                address = bytesAddressPair[1]
                clientMsg = "Message from Client:{}".format(message)
                clientIP  = "Client IP Address:{}".format(address)
                if message == "areyoucamera":
                    udpServerSocket.sendto(camera_id.encode("ascii"),address)
                    cameraLogger.info("Handshake UDP server was asked areyoucamera by {} and sent back iamcamera".format(address))  
                    newCore = True             
        except Exception as exception:
            cameraLogger.critical(exception.args)
        #Wait a bit before setting up the server again if an error occured
            

def image_udpserver():
    global newCore
    global coreIP
    global alertTime
    global record
    # 0.0.0.0 because this is a server
    localAddress = ("0.0.0.0", 13001)
    bufferSize  = 1024
    while True:
        try:
            # Create a datagram socket
            with socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM) as udpServerSocket:
                # Bind to address and ip
                udpServerSocket.bind(localAddress)
                print("Image UDP server up and listening")
                # Listen for incoming datagrams
                bytesAddressPair = udpServerSocket.recvfrom(bufferSize)
                message = bytesAddressPair[0].decode("ascii")
                address = bytesAddressPair[1]
                coreIP = address[0]
                #clientMsg = "Image function from Client:{}".format(message)
                #clientIP  = "Client IP Address:{}".format(address)
                #print(clientMsg)
                #print(clientIP)
                newCore=False
                
                #640*480 or 960*720
                cam = cv.VideoCapture(0)
                cam.set(cv.CAP_PROP_FRAME_WIDTH, 640)
                cam.set(cv.CAP_PROP_FRAME_HEIGHT, 480)
                cam.set(cv.CAP_PROP_FPS, 15)
                #To avoid recording when core goes down but cam is still up
                recording = False
                record = False
                size = (640,480)

                mount_videoFolder()   
                video_writer = None
                while newCore==False:
                    _,frame = cam.read() 
                    #for debugging
                    #start_time = time.time()
                    if not recording and detection_enabled:
                        detected = detect(frame)
                        if detected :
                            #Put some blue text on the frame
                            font = cv.FONT_HERSHEY_SIMPLEX
                            cv.putText(frame,'Detected !',(0,100), font, 1,(255,0,0),2,cv.LINE_AA)
                            alertTime = get_time_str()
                            cameraLogger.info("Intrusion detected !")
                    if recording and not record :
                        video_writer.release()
                        recording = False
                        cameraLogger.info("Recording ended")
                    elif not recording and record :
                        video_writer = cv.VideoWriter("/home/pi/coremount/DotNetCore/wwwroot/Alerts/"+recordPath+".mp4",cv.VideoWriter_fourcc(*'avc1'), 15, size)
                        recording = True
                        cameraLogger.info("Recording started")
                    encodingThread = threading.Thread(target=frame_encoding,args=(frame, udpServerSocket, address))
                    encodingThread.start()
                    if recording :               
                        videoWriterThread = threading.Thread(target=video_writing,args=(video_writer, frame))
                        videoWriterThread.start()
                        videoWriterThread.join()
                    #Wait for all threads to be done
                    encodingThread.join() 
                    #Only send one out of two images
                    #for debugging
                    #end_time = time.time()
        except Exception as exception:
            cameraLogger.critical(exception.args)
        finally:
            cam.release()
            udpServerSocket.close()

def load_config():
    global camera_id
    global detection_enabled
    global image_compressor
    global new_image_compressor
    global noise_reducer
    global detection_sensibility
    global detection_tolerance
    try:
        f = open("config.txt", "r")
        configLines = f.read().splitlines()
        camera_id = configLines[0]
        if configLines[1] == "True":
            detection_enabled = True
        else :
            detection_enabled = False
        image_compressor = float(configLines[2])
        new_image_compressor = float(configLines[2])
        noise_reducer = int(configLines[3])
        detection_sensibility = int(configLines[4])
        detection_tolerance = int(configLines[5])
        cameraLogger.info("Config loaded")
    except Exception as exception:
        cameraLogger.error(exception.args)

def save_config():
    try:
        f = open("config.txt", "w")
        toSave = str(camera_id)+"\n"+str(detection_enabled)+"\n"+str(new_image_compressor)+"\n"+str(noise_reducer)+"\n"+str(detection_sensibility)+"\n"+str(detection_tolerance)
        f.write(toSave)
        f.close()
        cameraLogger.info("Config saved")
    except Exception as exception:
        cameraLogger.error(exception.args)        

def data_server():
    global camera_id
    global detection_enabled
    global new_image_compressor
    global image_compressor
    global noise_reducer
    global detection_sensibility
    global detection_tolerance
    global record
    global recordPath

    localAddress = ("0.0.0.0", 13003)
    bufferSize  = 1024
    while True :
        try :
            with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as udpServerSocket:
                global newCore
                # Bind the socket to the port
                udpServerSocket.bind(localAddress)
                print("data server rdy")
                bytesAddressPair = udpServerSocket.recvfrom(bufferSize)
                address = bytesAddressPair[1]

                #We don't have to send camera_id because we already answer the handshake with it
                configData = str(detection_enabled)+"\n"+str(image_compressor)+"\n"+str(noise_reducer)+"\n"+str(detection_sensibility)+"\n"+str(detection_tolerance)
                udpServerSocket.sendto(configData.encode("ascii"), address)
                udpServerSocket.setblocking(True)
                udpServerSocket.settimeout(7)
                newCore = False
                while newCore == False:
                    configBytes = udpServerSocket.recv(bufferSize)
                    configData = configBytes.decode("ascii")
                    configList = configData.splitlines()
                    camera_id = configList[0]
                    if configList[1] == "True" :
                        detection_enabled = True
                    else :
                        detection_enabled = False
                    new_image_compressor = float(configList[2])
                    noise_reducer = int(configList[3])
                    detection_sensibility = int(configList[4])
                    detection_tolerance = int(configList[5])
                    if configList[6] == "True":
                        record = True
                    else:
                        record = False
                    recordPath = configList[7]
                    if configList[8] == "True":
                        save_config()
        except Exception as exception :
            cameraLogger.critical(exception.args)

def alert_server():
    localAddress = ("0.0.0.0", 13002)
    bufferSize  = 1024
    while True :
        try :
            with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as udpServerSocket:
                global newCore
                # Bind the socket to the port
                udpServerSocket.bind(localAddress)
                bytesAddressPair = udpServerSocket.recvfrom(bufferSize)
                address = bytesAddressPair[1]
                newCore = False

                while newCore == False:
                    udpServerSocket.sendto(alertTime.encode("ascii"), address)
                    time.sleep(1)
        except Exception as exception :
            cameraLogger.critical(exception.args)

def detect(frame):
    global baseline_image
    global base_time

    #Lowers the frame size then turns it gray and blurs it a little bit
    #Makes the detection faster and easier
    resized_frame = cv.resize(frame, (int(640*image_compressor), int(480*image_compressor)), interpolation=cv.INTER_CUBIC)
    gray_frame=cv.cvtColor(resized_frame,cv.COLOR_BGR2GRAY)
    gray_frame=cv.GaussianBlur(gray_frame,(noise_reducer,noise_reducer),0)
    #Updates the baseline frame every 10 seconds
    if baseline_image is None or time.time()-base_time >10:
        baseline_image=gray_frame
        base_time = time.time()
    #Calculates the absolute delta difference between each pixel of the baseline frame and the current frame
    delta=cv.absdiff(baseline_image,gray_frame)
    #Passes the result through a threshold filter which removes the values under detection_sensibility
    threshold=cv.threshold(delta, detection_sensibility, 255, cv.THRESH_BINARY)[1]
    #Calculates the mean delta 
    mean = threshold.mean()
    #Triggers a detection
    if mean>detection_tolerance:
        return True
    else:
        return False
    
def get_time_str():
    localtime = time.localtime()
    return str(localtime.tm_year)+":"+str(localtime.tm_mon)+":"+str(localtime.tm_mday)+":"+str(localtime.tm_hour)+":"+str(localtime.tm_min)+":"+str(localtime.tm_sec)

def mount_videoFolder():
    os.system("sudo mount -t cifs //"+coreIP+"/coredata /home/pi/coremount -o rw,dir_mode=0777,file_mode=0777,password=")
    if os.path.isdir("/home/pi/coremount/DotNetCore/wwwroot/Alerts") == False :
        raise NameError("Video folder not reachable nor mountable")

#Encodes the frame as a jpg image and sends it to the core        
def frame_encoding(frame, udpSocket, address):
    try :
        #encode_param = [int(cv.IMWRITE_JPEG_QUALITY), 25, int(cv.IMWRITE_JPEG_OPTIMIZE), 1, int(cv.IMWRITE_JPEG_PROGRESSIVE), 1]
        encode_param = [int(cv.IMWRITE_JPEG_QUALITY), 80]
        _,f = cv.imencode(".jpg", frame, encode_param)
        udpSocket.sendto(f, address)
    except Exception as exception:
        cameraLogger.critical(exception.args)

def video_writing(video_writer, frame):
    video_writer.write(frame)
