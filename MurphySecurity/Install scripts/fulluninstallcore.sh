sudo systemctl stop kestrel-murphysecurity.service
sudo systemctl disable kestrel-murphysecurity.service
sudo systemctl stop kestrel-murphysecuritypython.service
sudo systemctl disable kestrel-murphysecuritypython.service
cd /etc/systemd/system/
sudo rm kestrel-murphysecurity.service
sudo rm kestrel-murphysecuritypython.service
sudo apt remove -y samba
sudo cp -f /etc/default/smb.conf /etc/samba/
sudo rm -d -R /home/pi/coredata
sudo rm -d -R /home/pi/dotnet
python3 -m pip uninstall -y rpi-rf
sudo python3 -m pip uninstall -y rpi-rf
