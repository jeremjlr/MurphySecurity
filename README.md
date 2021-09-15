# MurphySecurity (v0.1)
MurphySecurity is a simple home security system designed to be used with Raspberry pi.

## Table of Contents
<ol>
  <li><a href="#about-the-project">About The Project</a></li>
  <li><a href="#features">Features</a></li>
  <li><a href="#pictures-videos">Pictures and Videos</a></li>
  <li>
      <a href="#installation">Installation</a>
      <ul>
        <li><a href="#installation-core">Core</a></li>
        <li><a href="#installation-cam">Cameras</a></li>
      </ul>
  </li>
  <li><a href="#roadmap">Roadmap</a></li>
  <li><a href="#license">License</a></li>
  <li><a href="#contact">Contact</a></li>
</ol>

## About The Project
<p align="center">
<img src="MurphySecurity/Media/MainScreen.png" width="480" height="270">
</p>
<p>MurphySecurity is a project that was initially started as a small experiment project to combine raspberry pis using python with a .NET CORE 3.1 web application made with Blazor.<br/>The goal was to have it run natively on a raspberry pi while other raspberry pis would be used as cameras using python scripts. Both entities communicate using UDP and TCP sockets on a private WiFi network.</p>
<p>Note that while I plan on making the system compatible with ONVIF IP cameras, I want to keep the possibilty to still have fully custom rpi cameras. So if I ever want to add a lot of image processing (face detection, object recognition, custom functionalities, etc..), I want to be able to do it fully camera side on the rpi camera and only send relevent data to the core. It was the main idea why I initially wanted the cameras to also be raspberry pis instead of just having a core rpi working with ONVIF cameras.</p>
<p>It ended up being a pretty interesting project combining different technical aspects (Blazor, Python, C#, Raspberry Pi/Linux, Networking, Movement detection, 433Mhz detectors, etc..) so I decided to push it to a functional state.<br/>This project can have an interesting educational purpose. If you plan on using it in such a way, please let me know.</p>
<p>The installation process might seem a bit tedious but it is acutally very straight forward.<br/>
I made a script for quick installations but if you're having trouble I also wrote a whole step-by-step guide in order to easily find where the issue is.
</p>

## Features
<p>As of now, MurphySecurity has the following non-exhaustive functionnal features :</p>
<ul>
  <li>One raspberry pi is used as the core application, it can be accessed by ip with any web browser.</li>
  <li>Both core and cameras auto restart in case of crash or power-cut.</li>
  <li>The system doesn't stop working/recording in case of internet failure.</li>
  <li>The system reconnects automatically once internet is back up.</li>
  <li>Easy plug&play once installation is done on the raspberry pis.</li>
  <li>The system has distant login/logout with automatic disconnect after 6 hours.</li>
  <li>Automatically sends an email with the new ip to access core if it has changed.</li>
  <li>Can send alerts to multiple emails.</li>
  <li>Camera recordings can be viewed/deleted from the web app.</li>
  <li>Camera recordings can be downloaded from the web app.</li>
  <li>Camera connection to core if fully automatic.</li>
  <li>Works with raspberry pi night vision cameras.</li>
  <li>The camera detection algorithm is customizable from the web app.</li>
  <li>Compatible with most 433Mhz detectors, alarms or keys. It will both receive signals from detectors/keys and send signals to trigger alarms.</li> 
  <li>3 type of alerts, intrusion alerts from 433Mhz detectors and camera detection, fire alerts from 433Mhz fire detectors, SOS alerts from keys' SOS buttons.</li>
  <li>Alerts are repeated (email sent again) every 3 minutes until dealt with.</li>
  <li>System can easily be turned on or off either from the web app or 433Mhz keys.</li>
  <li>It is very easy to add new 433Mhz devices.</li>
</ul>

## Pictures and Videos

## Installation
<p>
Both core and camera installations are assumed to be done on a fresh raspberry pi os install.<br/>
Both should also have the same private WiFi network saved for automated connection.<br/>
MurphySecurity was developped under python 3.7.3, it hasn't been tested with any other python version.<br/>
Do not forget to also forward ports 80 and 443 to the core raspberry pi which should be assigned a static IP.
</p>

## Core Quick Script Installation
<p>Open terminal and type :</p>

```
sudo raspi-config
```

<p>Go to "System Options > Hostname" and change "raspberrypi" to "securitycore".</p>
<p>Quit and reboot.</p>

#### .NET
<p>Download the Arm32 SDK from : https://dotnet.microsoft.com/download/dotnet/3.1</p>
<p>Go to the download folder, open terminal. (F4)<br/>Copy/paste the following lines (remember to change the DOTNET_FILE value to your file name):</p>

```
  DOTNET_FILE=xxxx
  export DOTNET_ROOT=$HOME/dotnet
  mkdir -p "$DOTNET_ROOT" && tar zxf "$DOTNET_FILE" -C "$DOTNET_ROOT"
  export PATH=$PATH:$DOTNET_ROOT
  dotnet --version
```

<p>Version should return 3.1.XXX</p>
<p>Download the latest release, unzip and run the core installation script.</p>

## Cameras Quick Script Installation
<p>Open terminal and type :</p>

```
sudo raspi-config
```

<p>Go to "System Options > Hostname" and change "raspberrypi" to "securitycam".</p>
<p>Then go to "Interface Options > Camera" and choose "Yes".</p>
<p>Quit and reboot.</p>
<p>Download the latest release, unzip and run the camera installation script.</p>
<p>[Optionnal]You shouldn't always need this part but if you're having troubles, you can always add the core raspberry pi static IP to the local hosts of the cameras by openning the following file :</p>

```
sudo nano /etc/hosts
```

<p>Then adding :</p>

```
X.X.X.X securitycore
```

<p>at the end of the file, X.X.X.X being the IP you assigned to the core.</p>
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ADD ERROR NAME FROM LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

## Core Step-by-Step Installation
<p>Open terminal and type :</p>

```
sudo raspi-config
```

<p>Go to "System Options > Hostname" and change "raspberrypi" to "securitycore".</p>
<p>Quit and reboot.</p>
<p>Open terminal and type :</p>

```
sudo apt-get update
sudo apt-get upgrade
sudo python3 -m pip install --upgrade pip
mkdir /home/pi/coredata
```

#### .NET
<p>Download the Arm32 SDK from : https://dotnet.microsoft.com/download/dotnet/3.1</p>
<p>Go to the download folder, open terminal. (F4)<br/>Copy/paste the following lines (remember to change the DOTNET_FILE value to your file name):</p>

```
DOTNET_FILE=xxxx
export DOTNET_ROOT=$HOME/dotnet
mkdir -p "$DOTNET_ROOT" && tar zxf "$DOTNET_FILE" -C "$DOTNET_ROOT"
export PATH=$PATH:$DOTNET_ROOT
sudo env "PATH=$PATH" dotnet dev-certs https --clean
sudo env "PATH=$PATH" dotnet dev-certs https
```

<p>Version should return 3.1.XXX</p>

#### Samba
<p>Open terminal and copy/paste those lines :</p>

```
sudo apt-get install samba
sudo nano /etc/samba/smb.conf
```

<p>Then copy/paste the following at the end of the openned file :</p>

```
[coredata]
comment = Pi Shared Folder
path = /home/pi/coredata
browsable = yes
writable = yes
guest ok = yes
read only = no
create mask = 0777
force create mode = 0777
directory mask = 0777
force directory mode = 0777
force user = root
```

<p>Save and exit.</p>

#### Files
<p>Download the latest release, unzip and place the folder in the coredata folder. Then place all the files in Python scripts/Core scripts/ in the coredata folder as well.</p>

#### Autostart on boot (Blazor)
<p>Open terminal, copy/paste the following :</p>
<p>sudo nano /etc/systemd/system/kestrel-murphysecurity.service</p>
<p>Then copy/paste the following in the openned file :</p>
<p>
[Unit]<br/>
Description=.NET Web API for Murphy Security<br/>
<br/>
[Service]<br/>
WorkingDirectory=/home/pi/coredata/publish<br/>
ExecStart=/home/pi/dotnet/dotnet /home/pi/coredata/publish/CameraMonitoringFrontEnd.dll<br/>
Restart=always<br/>
# Restart service after 10 seconds if the dotnet service crashes:<br/>
RestartSec=10<br/>
SyslogIdentifier=murphy-dotnet<br/>
User=root<br/>
<br/>
[Install]<br/>
WantedBy=multi-user.target
</p>
<p>Save and exit.</p>
<p>Finally, copy/paste the following lines :</p>
<p>
sudo systemctl enable kestrel-murphysecurity.service<br/>
sudo systemctl start kestrel-murphysecurity.service<br/>
sudo systemctl status kestrel-murphysecurity.service<br/>
</p>
<p>At this point the core web app is ready and the status should return up and running.</p>

#### rpi-rf
<p>We now have to setup the 433Mhz part.</p>
<p>Open terminal and copy/paste (upgrading pip3 first might be needed):</p>
<p>sudo pip3 install rpi-rf</p>
<p>
Open the GPIO Diagram file in the Python scripts/Core scripts/ folder.<br/>
Connect the 433Mhz receiver and transmitter like this :<br/>
<br/>
Receiver connections-<br/>
Ground : pin 20 (Ground)<br/>
+5V : pin 2 (5V power)<br/>
Data : pin 18 (GPIO 24)<br/>
<br/>
Transmitter connections-<br/>
Ground : pin 6 (Ground)<br/>
+5V : pin 4 (5V power)<br/>
Data : pin 37 (GPIO 26)<br/>
</p>

#### Autostart on boot (Python)
<p>Open terminal, copy/paste the following :</p>
<p>sudo nano /etc/systemd/system/kestrel-murphysecuritypython.service</p>
<p>Then copy/paste the following in the openned file :</p>
<p>
[Unit]<br/>
Description=Python3 scripts for Murphy Security<br/>
<br/>
[Service]<br/>
WorkingDirectory=/home/pi/coredata<br/>
ExecStart=python3 /home/pi/coredata/radioreceivercore.py<br/>
Restart=always<br/>
# Restart service after 10 seconds if the dotnet service crashes:<br/>
RestartSec=10<br/>
SyslogIdentifier=murphy-python<br/>
User=root<br/>
<br/>
[Install]<br/>
WantedBy=multi-user.target
</p>
<p>Save and exit.</p>
<p>Finally, copy/paste the following lines :</p>
<p>
sudo systemctl enable kestrel-murphysecuritypython.service<br/>
sudo systemctl start kestrel-murphysecuritypython.service<br/>
sudo systemctl status kestrel-murphysecuritypython.service<br/>
</p>
<p>At this point the radioreceiver is ready and the status should return up and running.</p>

## Cameras Step-by-Step Installation
<p>Open terminal and type :</p>
<p>sudo raspi-config</p>
<p>Go to "System Options > Hostname" and change "raspberrypi" to "securitycam".</p>
<p>Then go to "Interface Options > Camera" and choose "Yes".</p>
<p>Quit and reboot.</p>
<p>Open terminal and copy/paste those lines :</p>
<p>
sudo apt-get update<br/>
sudo apt-get upgrade<br/>
python3 -m pip install --upgrade pip<br/>
mkdir /home/pi/camdata<br/>
mkdir /home/pi/coremount
</p>

#### opencv-contrib-python and numpy
<p>Open terminal and copy/paste those lines :</p>
<p>sudo apt install libaec0 libaom0 libatk-bridge2.0-0 libatk1.0-0 libatlas3-base libatspi2.0-0 libavcodec58 libavformat58 libavutil56 libbluray2 libcairo-gobject2 libcairo2 libchromaprint1 libcodec2-0.8.1 libcroco3 libdatrie1 libdrm2 libepoxy0 libfontconfig1 libgdk-pixbuf2.0-0 libgfortran5 libgme0 libgraphite2-3 libgsm1 libgtk-3-0 libharfbuzz0b libhdf5-103 libilmbase23 libjbig0 libmp3lame0 libmpg123-0 libogg0 libopenexr23 libopenjp2-7 libopenmpt0 libopus0 libpango-1.0-0 libpangocairo-1.0-0 libpangoft2-1.0-0 libpixman-1-0 librsvg2-2 libshine3 libsnappy1v5 libsoxr0 libspeex1 libssh-gcrypt-4 libswresample3 libswscale5 libsz2 libthai0 libtheora0 libtiff5 libtwolame0 libva-drm2 libva-x11-2 libva2 libvdpau1 libvorbis0a libvorbisenc2 libvorbisfile3 libvpx5 libwavpack1 libwayland-client0 libwayland-cursor0 libwayland-egl1 libwebp6 libwebpmux3 libx264-155 libx265-165 libxcb-render0 libxcb-shm0 libxcomposite1 libxcursor1 libxdamage1 libxfixes3 libxi6 libxinerama1 libxkbcommon0 libxrandr2 libxrender1 libxvidcore4 libzvbi0<br/>
python3 -m pip install opencv-contrib-python<br/>
python3 -m pip install -U numpy
</p>

#### Files
<p>Download the latest release, unzip and place all the files in Python scripts/Camera scripts/ in the camdata folder.</p>

#### Core Hostname
<p>[Optionnal]You shouldn't always need this part but if you're having troubles, you can always add the core raspberry pi static IP to the local hosts of the cameras by openning the following file :</p>
<p>sudo nano /etc/hosts</p>
<p>Then adding :<br/>
X.X.X.X securitycore<br/>
at the end of the file, X.X.X.X being the IP you assigned to the core.</p>
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ADD ERROR NAME FROM LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

#### Autostart on boot (Python)
<p>Open terminal, copy/paste the following :</p>
<p>sudo nano /etc/systemd/system/kestrel-murphysecuritypythoncam.service</p>
<p>Then copy/paste the following in the openned file :</p>
<p>
[Unit]<br/>
Description=Python3 scripts for Murphy Security<br/>
<br/>
[Service]<br/>
WorkingDirectory=/home/pi/camdata<br/>
ExecStart=python3 /home/pi/camdata/main.py<br/>
Restart=always<br/>
# Restart service after 10 seconds if the dotnet service crashes:<br/>
RestartSec=10<br/>
SyslogIdentifier=murphy-python<br/>
User=pi<br/>
<br/>
[Install]<br/>
WantedBy=multi-user.target
</p>
<p>Save and exit.</p>
<p>Finally, copy/paste the following lines :</p>
<p>
sudo systemctl enable kestrel-murphysecuritypythoncam.service<br/>
sudo systemctl start kestrel-murphysecuritypythoncam.service<br/>
sudo systemctl status kestrel-murphysecuritypythoncam.service<br/>
</p>
<p>At this point the camera is ready and the status should return up and running.</p>

#### [Optionnal] If your camera module looks too "pinkish"
<p>Open :<br/>
sudo nano/boot/config.txt<br/>
Add at the end :<br/>
add awb_auto_is_greyworld=1
</p>

## Roadmap
<ul>
  <li>The things I'm currently working on :
    <ul>
      <li>Make the system fully compatible with all ONVIF IP cameras.</li>
      <li>Change the way camera stream is sent to the core to increase quality and lower bandwidth.</li>
    </ul>
  </li>
  <li>A non-exhaustive list of interesting features that could be added :
    <ul>
      <li>Finish implementing another solution than gmail to send emails.</li>
      <li>Being able to switch to 4G/5G network in case of internet failure.</li>
      <li>Add better logging to the C# code.</li>
      <li>Add the possibility for the rpi cameras to detect the 433Mhz signals as well and forward them to the core. That would increase the range for 433Mhz detectors/alarms.</li>
      <li>Add a visitor password to access the core without being able to modifiy its parameters.</li>
      <li>Make the automatic logout purely server side using temporary connection tokens of some sort.</li>
      <li>Add some information on the camera while recording (Date, time, camera ID).</li>
      <li>Implement a solution to add sound to cameras.</li>
    </ul>
  </li>
</ul>

## License
<p>MurphySecurity is available under AGPL-3.0 License.</p>
<p>Libraries used by MurphySecurity :</p>
<ul>
  <li>opencv-python under the MIT License and all its dependencies, see https://github.com/opencv/opencv-python.</li>
  <li>rpi-rf under the BSD License, see https://github.com/milaq/rpi-rf.</li>
</ul>

## Contact
<p>If you have any question or suggestion feel free to reach out to me at jerem.jlr@gmail.com.</p>
