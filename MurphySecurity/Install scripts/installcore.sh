sudo apt-get update
sudo apt-get -y upgrade
sudo python3 -m pip install --upgrade pip
python3 -m pip install --upgrade 
sudo python3 -m pip install rpi-rf
python3 -m pip install rpi-rf
mkdir /home/pi/coredata
cp -f -R PythonCore /home/pi/coredata
cp -f -R DotNetCore /home/pi/coredata
DOTNET_FILE=dotnet-sdk.gz
export DOTNET_ROOT=$HOME/dotnet
mkdir -p "$DOTNET_ROOT" && tar zxf "$DOTNET_FILE" -C "$DOTNET_ROOT"
export PATH=$PATH:$DOTNET_ROOT
sudo env "PATH=$PATH" dotnet dev-certs https --clean
sudo env "PATH=$PATH" dotnet dev-certs https
sudo apt install -y samba
sudo cp -f /etc/samba/smb.conf /etc/default/
sudo tee -a /etc/samba/smb.conf <<<"
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
force user = root"
sudo tee /etc/systemd/system/kestrel-murphysecurity.service <<<"[Unit]
Description=.NET Web API for Murphy Security

[Service]
WorkingDirectory=/home/pi/coredata/DotNetCore
ExecStart=/home/pi/dotnet/dotnet /home/pi/coredata/DotNetCore/MurphySecurity.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=murphy-dotnet
User=root

[Install]
WantedBy=multi-user.target"
sudo systemctl enable kestrel-murphysecurity.service
sudo systemctl start kestrel-murphysecurity.service
sudo tee /etc/systemd/system/kestrel-murphysecuritypython.service <<<"[Unit]
Description=Python3 scripts for Murphy Security

[Service]
WorkingDirectory=/home/pi/coredata/PythonCore
ExecStart=python3 /home/pi/coredata/PythonCore/radioreceivercore.py
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=murphy-python
User=pi

[Install]
WantedBy=multi-user.target"
sudo systemctl enable kestrel-murphysecuritypython.service
sudo systemctl start kestrel-murphysecuritypython.service
cp -f uninstallcore.sh /home/pi/coredata

