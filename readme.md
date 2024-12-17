### Installation instructions
The following commands assume your user is `pi` and you are in the home directory. This will be the case if you set up a raspberry pi with the default setting of having `pi` as a user and then connect to the pi with ssh. If you are not in the home directory, you can navigate there with `cd ~`.

A bunch of commands / chain of commands will be given here. They have to be run in the correct order. You can copy and paste the whole command chain at once for each command chain.

First, install .net 8 with this commands (will reboot):
```bash
sudo apt update && sudo apt install git -y && \
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --verbose && \
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc && \
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc && \
sudo reboot;
```
Then, clone the repository and build it (this can take a minute or two):
```bash
git clone https://github.com/AdamTovatt/receipt-printer.git && \
cd receipt-printer && \
dotnet publish -c Release -r linux-arm64;
```

Then, setup the printer with the following commands (this is assuming you're using a star micronics mc print2 and is assuming you haven't moved the terimal since the last commands) (will take a while and should print a text saying it works at the end):
```bash	
sudo apt-get update && \
sudo apt-get install cups -y && \
sudo usermod -a -G lpadmin pi && \
cd ReceiptPrinter/Resources && \
sudo cp Star_CUPS_Driver-3.16.0_linux.tar.gz ../../../Star_CUPS_Driver-3.16.0_linux.tar.gz && \
cd ../../../ && \
tar xzvf Star_CUPS_Driver-3.16.0_linux.tar.gz && \
sudo rm Star_CUPS_Driver-3.16.0_linux.tar.gz && \
cd Star_CUPS_Driver-3.16.0_linux/SourceCode && \
tar xzvf Star_CUPS_Driver-src-3.16.0.tar.gz && \
cd Star_CUPS_Driver/ && \
sudo apt-get install gcc && \
sudo apt-get install libcups2-dev -y && \
sudo apt-get install libcupsimage2-dev && \
sudo make && \
sudo make install && \
sudo systemctl start cups && \
sudo systemctl enable cups && \
sudo lpadmin -p mC_Print2 -E -v "usb://Star/MCP21%20(STR-001)?serial=$(sudo lpinfo -v | grep -oP 'usb://Star/[^?]*\?serial=\K[0-9]+')" -m /star/mcp21.ppd && \
cd ../../../ && \
lpoptions -d mC_Print2 && \
echo $'~~~~~~~~~~~~~~~~~\nThe printer works\n~~~~~~~~~~~~~~~~~\n\n.' > file.txt && sudo lp -d mC_Print2 file.txt && rm file.txt;
```

Now you need to know "client-id" and "client-secret". These can be obtained from zettle. You also need to answer if the printer is for customers or not. The system is built to have two printers, one in the kitchen and one where customers order food.

Create the correct service unit file, enable and start it. That can be done with the following commands (will ask for input):
```bash
echo "Enter the client-id:" && \
read client_id && \
client_id=$(echo "$client_id" | xargs) && \
echo "Enter the client-secret:" && \
read client_secret && \
client_secret=$(echo "$client_secret" | xargs) && \
while true; do \
    echo "Is it for customer? (true/false):" && \
    read is_for_customer && \
    is_for_customer=$(echo "$is_for_customer" | xargs) && \
    if [[ "$is_for_customer" == "true" || "$is_for_customer" == "false" ]]; then break; else \
    echo "Invalid input. Please enter 'true' or 'false'."; fi; done && \
sudo bash -c "cat <<EOF > /lib/systemd/system/receipt-printer.service
[Unit]
Description=Receipt Printer
After=nginx.service

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/receipt-printer
ExecStart=/home/pi/receipt-printer/ReceiptPrinter/bin/Release/net8.0/linux-arm64/publish/ReceiptPrinter --client-id $client_id --client-secret $client_secret --allowed-categories mat --font-size 12 --page-width-mm 48 --is-for-customer $is_for_customer --bottom-margin 3 --bottom-decoration ~ --top-decoration ~
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ROOT=/home/pi/.dotnet

[Install]
WantedBy=multi-user.target
EOF" && \
echo "The receipt-printer.service file has been created with the provided values." && \
sudo systemctl daemon-reload && \
sudo systemctl enable receipt-printer.service && \
sudo systemctl start receipt-printer.service;
```
After the previous commands have been run the printer should come to life.

Next, create the service that prints the local ip on startup, this is so that we don't have to try to use an arp table from the router to find it if we need to change something later:
```bash
sudo bash -c 'cat <<EOF > /usr/local/bin/print-ip.sh
#!/bin/bash
sleep 30
LOCAL_IP=\$(hostname -I | awk "{print \$1}")
echo -e "~~~~~~~~~~~~~~~~~\n\$LOCAL_IP\n~~~~~~~~~~~~~~~~~\n\n." > /tmp/ip-address.txt
sudo lp -d mC_Print2 /tmp/ip-address.txt
rm /tmp/ip-address.txt
EOF' && \
sudo chmod +x /usr/local/bin/print-ip.sh && \
sudo bash -c 'cat <<EOF > /lib/systemd/system/print-ip-on-boot.service
[Unit]
Description=Print Local IP Address on Startup
After=network-online.target

[Service]
Type=simple
ExecStart=/usr/local/bin/print-ip.sh
Restart=no

[Install]
WantedBy=multi-user.target
EOF' && sudo systemctl daemon-reload && \
sudo systemctl enable print-ip-on-boot.service && \
sudo systemctl start print-ip-on-boot.service;
```

Around 30 seconds after the previous commands are run the printer should print the local ip address.

Now, create the service that keeps the printing service updated in case any changes are pushed to the git repository:
```bash
echo -e '#!/bin/bash\nsudo systemctl stop receipt-printer' | sudo tee /home/pi/stop-receipt-printer.sh > /dev/null && \
echo -e '#!/bin/bash\nsudo systemctl start receipt-printer' | sudo tee /home/pi/start-receipt-printer.sh > /dev/null && \
sudo chmod +x /home/pi/stop-receipt-printer.sh && sudo chmod +x /home/pi/start-receipt-printer.sh && \
git clone https://github.com/AdamTovatt/auto-updater.git && cd auto-updater && dotnet publish -c Release -r linux-arm64 && cd .. && \
cat <<EOF > receipt-printer-update-script.txt
cd /home/pi/receipt-printer;
git pull;
contains Updating;
/home/pi/stop-receipt-printer.sh;
/home/pi/.dotnet/dotnet publish ReceiptPrinter/ReceiptPrinter.csproj -c Release -r linux-arm64;
/home/pi/start-receipt-printer.sh;
EOF
echo "Created receipt-printer-update-script.txt" && \
sudo bash -c 'cat <<EOF > /lib/systemd/system/receipt-printer-auto-update.service
[Unit]
Description=Receipt Printer Auto Update
After=nginx.service

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/auto-updater
ExecStart=/home/pi/auto-updater/AutoUpdater/bin/Release/net8.0/linux-arm64/AutoUpdater --script-path /home/pi/receipt-printer-update-script.txt --logging false
Restart=always
RestartSec=40
Environment=DOTNET_ROOT=/home/pi/.dotnet

[Install]
WantedBy=multi-user.target
EOF
echo "receipt-printer-auto-update.service file created successfully, will enable service."'
sudo systemctl daemon-reload && \
sudo systemctl enable receipt-printer-auto-update.service && \
sudo systemctl start receipt-printer-auto-update.service;
```

Now the last step is to connect it to the wifi that it will use (if that's not the one it's already connected too). The following commands will ask for a name for the new connection (doesn't really matter what it is but it should probably be unique and understandable), the SSID of the wifi and the password for the wifi. When the new connection has been successfully created the old connection will be deleted. Which is a bit scary becaues it means you will loose connection if you are using ssh. But that's why we fixed the automatic local ip address reporting earlier. This means that after you run these commands, if they are successfull in setting up the new connection the ssh connetion will freeze and then dissappear. Then you can wait a little while and unplug the device. Then plug it in again and it should restart with the new connection and report the new local ip. The commands are:
```bash
read -p "Enter connection name: " con_name
read -p "Enter SSID: " ssid
read -p "Enter WPA2 password: " password

sudo nmcli connection add type wifi ifname wlan0 con-name "$con_name" ssid "$ssid" && \
sudo nmcli connection modify "$con_name" wifi-sec.key-mgmt wpa-psk && \
sudo nmcli connection modify "$con_name" wifi-sec.psk "$password" && \
sudo nmcli connection up "$con_name" && \
sudo nmcli connection delete preconfigured
```

If this was successfull, the last thing you will see before it freezes is "Connection '[name]' ([uuid]) successfully added.".

Then you should not get back the ssh connection.
If you type the wrong password it can happen that it freezes temporarily but then says that password or secrets were not given. If that happens you can delete the connection you just created `nmcli connection delete "name"
` and then run the commands again.