#!/usr/bin/bash

source ./dependency_android.sh

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
DEPLOY_TARGET_ID="\*"
UNIT_TEST_CONNECT_RETRY_COUNT=10

# NOTE(ruel): receive argument and organize
while getopts ":f:m:p:c:vp:i:" opt; do
	case $opt in
		:)
			echo "Usage: -f {APK_PATH}, -i {DEVICE_ID} -m [ETH,USB(default)], -c [main activity name:default(com.unity3d.player.UnityPlayerActivity)] -v(verbose)"
			exit 1
			;;
		v)
			VERBOSE="TRUE"
			;;
		f)
			APK=$OPTARG
			;;
		i)
			DEPLOY_TARGET_ID=$OPTARG
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

source ./safe_exit.sh

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

ADB_EN_ADDR=$(source ./list_android_wifi_devices.sh | awk '{split($0,a,":"); print a[1]}')
ADB_USB_DEVICES=$(source ./list_android_usb_devices.sh)

#LOCAL_IPS_HEAD=$(ifconfig | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | grep -v ^127 |awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
DEVICE_IDS=$ADB_USB_DEVICES

# NOTE(ruel): check argument configured
if [ ! -v APK ]; 
then 
	echo "Error: -f {APK_PATH} option required" 1>&2
	safe_exit 1
fi

# NOTE(ruel): check file exists
if [ ! -f $APK ];
then	
	echo "Error: "$APK" is not exists!" 1>&2
	safe_exit 1
fi

BUNDLE_ID=$(aapt dump badging $APK | awk '/package/{gsub("name=|'"'"'","");  print $2}')
BUNDLE_ID_ACTIVITIES=$(aapt dump badging $APK | awk '/activity/{gsub("name=|'"'"'","");  print $2}' | sort | uniq)

if [ -z $BUNDLE_ID ]
then
	echo "Error: "$APK" is not Android Application Package(APK). or unable to retrieve bundle identifier" 1>&2
	safe_exit 1
fi 

if [ ! -v MAIN_ACTIVITY ]
then
	MAIN_ACTIVITY=$(echo $BUNDLE_ID_ACTIVITIES | tr ' ' '\n' | head -n 1)
fi 

echo "APK file to install: $APK, Bundle Identifier: $BUNDLE_ID, Activity: $MAIN_ACTIVITY"

for DEVICE_ID in $DEVICE_IDS;
do

	if [ $DEPLOY_TARGET_ID == "\*" ] || [ $DEVICE_ID == $DEPLOY_TARGET_ID ]
	then
		echo "Try uninstalling $BUNDLE_ID from $DEVICE_ID ..."
		adb -s $DEVICE_ID uninstall $BUNDLE_ID

		echo "Installing $BUNDLE_ID into $DEVICE_ID ..."
		adb -s $DEVICE_ID install "$APK"
		
		# NOTE(ruel) 3 means 3 is home. some devices restrict const definitions (like KEYCODE_HOME)
		adb -s $DEVICE_ID shell input keyevent 3

		echo "Launching $BUNDLE_ID of $DEVICE_ID ..."
		adb -s $DEVICE_ID shell am start -n "$BUNDLE_ID/$MAIN_ACTIVITY"

		# get ips of DEVICE_ID that intersect
		IP_ADDRS=$(source ./list_android_ip.sh -i $DEVICE_ID)

		if [ -z "$IP_ADDRS" ]
		then
			echo "unable to detect any matched ip to connect unit test servers"
		else  
			# try connect to unit test server with each ips
			i=$UNIT_TEST_CONNECT_RETRY_COUNT # try UNIT_TEST_CONNECT_RETRY_COUNT times for each ip
			LISTEN="FALSE"
			while [ $LISTEN == "FALSE" ];
			do	
				sleep 1

				for IP_ADDR in $IP_ADDRS;
				do 
					if [ $LISTEN == "FALSE" ];
					then 
						echo "Try Connecting $IP_ADDR:7701"
						nc -w 2 -v $IP_ADDR 7701 </dev/null; TEST_RESULT=$?;
						if [ "$TEST_RESULT" -eq 0 ]
						then
							LISTEN="TRUE"
							echo "$IP_ADDR:7701 is reachable. we're going to test."
							AVAILABLE_IP_ADDR=$IP_ADDR
						else 
							echo "$IP_ADDR:7701 is not opened yet..."
						fi 
					fi 
				done

				if [ $((i--)) -lt 0 ]
				then 
					echo "Error: Out of retry count" 1>&2
					safe_exit 1
				fi
			done

			if [ -v AVAILABLE_IP_ADDR ];
			then
				python client.py -a $AVAILABLE_IP_ADDR -p 7701
			else 
				echo "no available ip to unit test"
			fi
		fi 
	fi 
done

safe_exit 0
