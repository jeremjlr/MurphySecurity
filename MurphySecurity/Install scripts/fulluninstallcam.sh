python3 -m pip uninstall -y opencv-contrib-python
python3 -m pip uninstall -y numpy
sudo systemctl stop kestrel-murphysecuritypythoncam.service
sudo systemctl disable kestrel-murphysecuritypythoncam.service
cd /etc/systemd/system/
sudo rm kestrel-murphysecuritypythoncam.service
sudo rm -d -R /home/pi/camdata
sudo umount /home/pi/coremount
sudo rm -d -R /home/pi/coremount