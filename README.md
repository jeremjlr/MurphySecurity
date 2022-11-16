# MurphySecurity (v0.1.2.0)
MurphySecurity is a simple home security system designed to be used with Raspberry pis.<br/>
You can monitor your home, setup a camera with movement detection, setup multiple detection devices (door sensors, movement sensors, smoke detectors, etc), receive emails when any alert is triggered (intrusion, fire or SOS alerts), access and control everything from anywhere only using a web browser thanks to a .NET Blazor web application, or even setup a remote key to easily turn on or off the system as well as launch SOS alerts... all of that with (at least) a single Raspberry Pi :)

## Table of Contents
<ol>
  <li><a href="#about-the-project">About The Project</a></li>
  <li><a href="#features">Features</a></li>
  <li><a href="#installation">Installation</a></li>
  <li><a href="howtousevideos">How to Use / Videos</a></li>
  <li><a href="#roadmap">Roadmap</a></li>
  <li><a href="#faq">FAQ</a></li>
  <li><a href="#license">License</a></li>
  <li><a href="#contact">Contact</a></li>
</ol>

## About The Project
<p>MurphySecurity is a project that was initially started as a small experiment project to combine raspberry pis using python scripts with a .NET CORE 3.1 web application made with Blazor.<br/>The goal was to have the Blazor app run natively on a raspberry pi while other raspberry pis would be used as cameras using python scripts. Both entities communicate using UDP and TCP sockets on a private WiFi network.<br/>
However, if you prefer, you can install both the core and camera modules on the same pi and it will work just fine.</p>
<p>Note that while I plan on making the system compatible with ONVIF IP cameras, I want to keep the possibilty to still have fully custom rpi cameras. So if I ever want to add a lot of custom image processing (face detection, object recognition, custom functionalities, etc..), I want to be able to do it fully camera side on the rpi camera and only send relevent data to the core. It was the main reason why I initially wanted the cameras to also be raspberry pis instead of just having a core rpi working with ONVIF cameras.</p>
<p>It ended up being a pretty interesting project combining different technical aspects (Blazor, Python, C#, Raspberry Pi/Linux, Networking, Movement detection, 433Mhz detectors, etc..) so I decided to push it to a functional state.<br/>This project can have an interesting educational purpose. If you plan on using it in such a way, please let me know.</p>
<p>I made scripts to install/uninstall everything so while the installation process might seem a bit tedious, it is acutally very straight forward.</p>

## Features
As of now, MurphySecurity has the following non-exhaustive functionnal features :
<ul>
  <li>One raspberry pi is used as the core device, it can be accessed by ip with any web browser.</li>
  <li>Both core and cameras auto restart in case of crash or power-cut.</li>
  <li>A single raspberry pi can be used as both core and camera at the same time.</li>
  <li>The system doesn't stop working/recording in case of internet failure.</li>
  <li>The system reconnects automatically once internet is back up.</li>
  <li>Easy plug&play once installation is done on the raspberry pis.</li>
  <li>Camera connection to the core is fully automated, it only needs to be on the same network.</li>
  <li>Very easy to setup thanks to installation scripts.</li>
  <li>The system has distant login/logout with automated disconnection after 6 hours.</li>
  <li>Automatically sends an email with the new ip to access the web app if it has changed.</li>
  <li>Can send alerts to multiple emails.</li>
  <li>Camera recordings can be viewed/deleted/downloaded from the web app.</li>
  <li>Works with raspberry pi night vision cameras and any usb camera.</li>
  <li>The camera detection algorithm is customizable from the web app.</li>
  <li>Compatible with most 433Mhz detectors, alarms or keys. It will both receive signals from detectors/keys and send signals to trigger alarms.</li> 
  <li>There are 3 types of alerts, intrusion alerts from 433Mhz detectors and camera detection, fire alerts from 433Mhz smoke detectors, SOS alerts from keys' SOS buttons.</li>
  <li>Alerts are repeated (email sent again) every 3 minutes until dealt with.</li>
  <li>The system can easily be turned on or off either from the web app or 433Mhz keys.</li>
  <li>It is very easy to add new 433Mhz devices.</li>
</ul>

## Installation
Both core and camera installations are assumed to be done on a fresh raspberry pi os install.<br/>
Both should also have the same private WiFi network saved for automated connection.<br/>
MurphySecurity was developped under python 3.7.3, it hasn't been tested with any other python version.<br/>
Do not forget to also forward ports 80 and 443 to the core raspberry pi, to which you should also assign a static IP.

### [Core only] 433Mhz Receiver/Transmitter GPIO Connections
If you're installing a core don't forget to set up the GPIO 433Mhz receiver and transmitter using the following instructions :
Open the GPIO Diagram file in the PythonCore folder.
Turn off your pi then connect the 433Mhz receiver and transmitter like this:


```
Receiver connections-
Ground : pin 20 (Ground)
+5V : pin 2 (5V power)
Data : pin 18 (GPIO 24)

Transmitter connections-
Ground : pin 6 (Ground)
+5V : pin 4 (5V power)
Data : pin 37 (GPIO 26)
````

### Pi Configuration
<p>Open terminal and type :</p>

```
sudo raspi-config
```

<p>Go to "System Options > Hostname" and change "raspberrypi" to "securitycore"/"securitycam" or whatever name you want.</p>
<b>[Camera only]</b>
<p>If you're installing a cam also don't forget to enable the camera by going to "Interface Options > Camera" and setting it to "Yes".</p>
<p>Quit and reboot.</p>

### Installation Scripts
Download the latest release, unzip, open terminal and run the script of your choice. There are scripts to install and uninstall both core and camera.<br/>
i.e :

```
bash installcore.sh
```

## How to Use / Videos
Using emails to send alerts was prefered due to its flexibility, it is easy to set up notifications when an email is received on any device. Especially on a smartwatch which i'm using to receive instant notifications.

Some of the devices this project was tested with :
<p>
            <img width=200 height=266 src="https://github.com/jeremjlr/MurphySecurity/blob/work-branch/MurphySecurity/Media/All%20devices.jpg"/>
            <img width=200 height=266 src="https://github.com/jeremjlr/MurphySecurity/blob/work-branch/MurphySecurity/Media/Transmitter-Receiver.jpg"/>
            <img width=153 height=202 src="https://github.com/jeremjlr/MurphySecurity/blob/work-branch/MurphySecurity/Media/SmartWatch.jpg"/>
</p>


Some videos showing a few random things using the webapp on a phone : 
<ul>
            <li>Camera tour : https://www.youtube.com/watch?v=_s6VhPigDJ0</li>
            <li>Detectors : https://www.youtube.com/watch?v=h0LoaUNL5Jw</li>
            <li>Camera detection : https://www.youtube.com/watch?v=taBiFCX9Vc8</li>
            <li>Alerts : https://www.youtube.com/watch?v=02KXFaCYDvs</li>
            <li>Emails : https://www.youtube.com/watch?v=_kfk0imHzMo</li>
</ul>

Everything should be pretty straight forward but here are some instructions that might help you use the app.

First, you need an email alert sender, which should be a gmail address with less secure apps access enabled.<br/>
You can enable it here : https://myaccount.google.com/security<br/>
This will be fixed in further releases.<br/>
You can then add the email sender on the security app under the emails tab.<br/>
Also add at least one email for alerts to be sent to.<br/>
Now click the send test email alert button and make sure you received it.

You can enable/disable camera detection as well as customize the detection algorithm in the camera options.<br/>
I recommend leaving image compression to 0.5 and noise reducer to 5.<br/>
Here is the algorithm for those interested :

```python
def detect(frame):
    global baseline_image
    global base_time
    global detection_count

    #Lowers the frame size then turns it gray and blurs it a little bit
    #Makes the detection faster and easier
    resized_frame = cv.resize(frame, (int(640*image_compressor), int(480*image_compressor)), interpolation=cv.INTER_CUBIC)
    gray_frame=cv.cvtColor(resized_frame,cv.COLOR_BGR2GRAY)
    gray_frame=cv.GaussianBlur(gray_frame,(noise_reducer,noise_reducer),0)
    #Updates the baseline frame every 10 seconds
    if baseline_image is None or time.time()-base_time >10:
        baseline_image=gray_frame
        base_time = time.time()
        detection_count = 0
    #Calculates the absolute delta difference between each pixel of the baseline frame and the current frame
    delta=cv.absdiff(baseline_image,gray_frame)
    #Passes the result through a threshold filter which removes the values under detection_sensibility
    threshold=cv.threshold(delta, detection_sensibility, 255, cv.THRESH_BINARY)[1]
    #Calculates the mean delta 
    mean = threshold.mean()
    #Triggers a detection
    if mean>detection_tolerance:
        detection_count += 1
        baseline_image = gray_frame
        if detection_count > frame_tolerance :
            return True
        else :
            return False
    else:
        return False
```

Adding new 433Mhz detectors/keys is very simple. Just go to the detectors tab and make your device emit its signals (press the keys' buttons, open/close a door detector, etc...) and the system should detect it automatically. You can then choose a name and a behavior and add it.

You can set up alarms by following the manufacturer instructions and going to the parameters tab to emit the core's 433Mhz code whenever it is requiered.<br/>
Once it is set up, enabling/disabling security should also trigger a little alarm bip.<br/>
There is also a button to generate new codes in case of conflict.

## Roadmap
<ul>
  <li>The things I'm currently working on :
    <ul>
      <li>Make the system fully compatible with all ONVIF IP cameras.</li>
      <li>Change the way camera stream is sent to the core to increase quality and stability.</li>
    </ul>
  </li>
  <li>A non-exhaustive list of interesting features that could be added :
    <ul>
      <li>Finish implementing a better solution to send emails.</li>
      <li>Add better logging to the C# code.</li>
      <li>Add the possibility for the rpi cameras to detect the 433Mhz signals as well and forward them to the core. That would increase the range for 433Mhz detectors/alarms.</li>
      <li>Add a visitor password to access the core without being able to modifiy its parameters.</li>
      <li>Make the automatic logout purely server side using temporary connection tokens of some sort.</li>
      <li>Add some information on the camera while recording (Date, time, camera ID).</li>
      <li>Implement a solution to add sound to the cameras.</li>
    </ul>
  </li>
</ul>

## Known Bugs

- Security Breach : For testing purposes, some files are stored in wwwroot and can be accessed without typing the password. They will be relocated.

## FAQ
<b>My camera looks too pinkish, what can I do ?</b><br/>
This has nothing to do with this project but here is a simple way to somewhat fix it, open:

```
sudo nano/boot/config.txt
```

Add at the end:<br/>

```
awb_auto_is_greyworld=1
```

<p><b>How come my 433Mhz receivers and transmitters do not seem to be working well ? They have very short range.</b><br/>
Please make sure you did not forget to weld the antenna.</p>

## License
MurphySecurity is available under AGPL-3.0 License.<br/>
Libraries used by MurphySecurity :<br/>
<ul>
  <li>opencv-python under the MIT License and all its dependencies, see https://github.com/opencv/opencv-python.</li>
  <li>rpi-rf under the BSD License, see https://github.com/milaq/rpi-rf.</li>
</ul>

## Contact
If you have any question or suggestion feel free to reach out to me at jerem.jlr@gmail.com.
