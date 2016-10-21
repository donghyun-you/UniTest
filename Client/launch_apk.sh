#!/usr/bin/bash

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
PACKAGE="com.ruel.unitest"
MAIN_ACTIVITY="com.unity3d.player.UnityPlayerActivity"

# NOTE(ruel): receive argument and organize
while getopts ":f:m:p:c:vp:" opt; do
	case $opt in
		:)
			echo "Usage: -f {APK_PATH}, -m [ETH,USB(default)], -p [Package name:default(com.ruel.unitest)] -c [main activity name:default(com.unity3d.player.UnityPlayerActivity)] -v(verbose)"
			exit 1
			;;
		v)
			VERBOSE="TRUE"
			;;
		f)
			APK=$OPTARG
			;;
		p)
			PACKAGE=$OPTARG
			;;
		c)
			MAIN_ACTIVITY=$OPTARG
			;;
		m)
			if [ "$OPTARG" == "ETH" ];
			then
				DEVICE_IDS=$ADB_EN_ADDR
				echo "Install and test into Ethernet over devices";
			fi
			;;
			
	esac
done

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

ADB_EN_ADDR=$(adb devices | grep -E '^[0-9\.]{8,16}' | awk '{print $1}' | awk '{split($0,a,":"); print a[1]}')
ADB_USB_DEVICES=$(adb devices | grep -E '^[0-9a-zA-Z]{16}' | awk '{print $1}')


LOCAL_IPS_HEAD=$(ifconfig | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | grep -v ^127 |awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
DEVICE_IDS=$ADB_USB_DEVICES

# NOTE(ruel): check argument configured
if [ ! -v APK ]; 
then 
	echo "Error: -a {APK_PATH} option required";
	exit 0;
fi

# NOTE(ruel): check file exists
if [ ! -f $APK ];
then	
	echo $APK" is not exists!"
	exit 0;
fi

echo "APK file to install: $APK"

for DEVICE_ID in $DEVICE_IDS;
do
	SCREENSHOT=$(echo "/tmp/"$DEVICE_ID".screen.png")
	SCREENSHOT_REMOTE_TEMP=$(echo "/sdcard/"$DEVICE_ID".screen.png")

	IP_ADDRS=$(adb -s $DEVICE_ID shell ip addr show | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | grep -v ^127)
	IP_ADDRS_HEAD=$(echo $IP_ADDRS | awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
	IP_ADDRS_INTERSECT=$(echo $LOCAL_IPS_HEAD $IP_ADDRS_HEAD | tr ' ' '\n' | sort | uniq -d)

	#adb -s $DEVICE_ID shell input keyevent 26 #Pressing the lock button
	#adb -s $DEVICE_ID shell input keyevent 66 #Pressing Enter
	adb -s $DEVICE_ID shell input keyevent KEYCODE_HOME

	echo "Try uninstalling $PACKAGE ..."
	adb -s $DEVICE_ID uninstall $PACKAGE 
	adb -s $DEVICE_ID install "$APK"

	adb -s $DEVICE_ID shell am start -n "$PACKAGE/$MAIN_ACTIVITY"

	LISTEN="FALSE"
	while [ $LISTEN == "FALSE" ];
	do	
		sleep 1
		LISTENING=$(adb -s 015d490628101c0a shell netstat | grep :7701 | grep -o LISTEN)
		if [ "$LISTENING" == "LISTEN" ];
		then
			LISTEN="TRUE"
			echo "7701 server is up. invoke the unit test"
		else
			echo "7701 server is not launched yet. continue awaiting..."
		fi
	done

	for IP_ADDR_INTERSECT in $IP_ADDRS_INTERSECT;
	do
		IP_ADDR=$(echo $IP_ADDRS | grep $IP_ADDR_INTERSECT)
		python client.py -a $IP_ADDR -p 7701
	done
	
	adb -s $DEVICE_ID shell screencap -p $SCREENSHOT_REMOTE_TEMP
	adb -s $DEVICE_ID pull $SCREENSHOT_REMOTE_TEMP $SCREENSHOT
	adb -s $DEVICE_ID shell rm $SCREENSHOT_REMOTE_TEMP
	open $SCREENSHOT
done

if [ "$VERBOSE" == "TRUE" ];
then
	set +x;
fi
