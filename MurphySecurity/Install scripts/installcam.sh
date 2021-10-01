sudo apt-get update
sudo apt-get -y upgrade
python3 -m pip install --upgrade pip
sudo apt install -y libaec0 libaom0 libatk-bridge2.0-0 libatk1.0-0 libatlas3-base libatspi2.0-0 libavcodec58 libavformat58 libavutil56 libbluray2 libcairo-gobject2 libcairo2 libchromaprint1 libcodec2-0.8.1 libcroco3 libdatrie1 libdrm2 libepoxy0 libfontconfig1 libgdk-pixbuf2.0-0 libgfortran5 libgme0 libgraphite2-3 libgsm1 libgtk-3-0 libharfbuzz0b libhdf5-103 libilmbase23 libjbig0 libmp3lame0 libmpg123-0 libogg0 libopenexr23 libopenjp2-7 libopenmpt0 libopus0 libpango-1.0-0 libpangocairo-1.0-0 libpangoft2-1.0-0 libpixman-1-0 librsvg2-2 libshine3 libsnappy1v5 libsoxr0 libspeex1 libssh-gcrypt-4 libswresample3 libswscale5 libsz2 libthai0 libtheora0 libtiff5 libtwolame0 libva-drm2 libva-x11-2 libva2 libvdpau1 libvorbis0a libvorbisenc2 libvorbisfile3 libvpx5 libwavpack1 libwayland-client0 libwayland-cursor0 libwayland-egl1 libwebp6 libwebpmux3 libx264-155 libx265-165 libxcb-render0 libxcb-shm0 libxcomposite1 libxcursor1 libxdamage1 libxfixes3 libxi6 libxinerama1 libxkbcommon0 libxrandr2 libxrender1 libxvidcore4 libzvbi0
python3 -m pip install -U numpy
python3 -m pip install opencv-contrib-python
mkdir /home/pi/camdata
mkdir /home/pi/coremount
cp -f -R PythonCam /home/pi/camdata
sudo tee /etc/systemd/system/kestrel-murphysecuritypythoncam.service <<<"[Unit]
Description=Python3 scripts for Murphy Security

[Service]
WorkingDirectory=/home/pi/camdata/PythonCam
ExecStart=python3 /home/pi/camdata/PythonCam/main.py
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=murphy-python
User=pi

[Install]
WantedBy=multi-user.target"
sudo systemctl enable kestrel-murphysecuritypythoncam.service
sudo systemctl start kestrel-murphysecuritypythoncam.service
cp -f uninstallcam.sh /home/pi/camdata
