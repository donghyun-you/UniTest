#!/bin/bash

source ./dependency_android.sh

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
DEPLOY_TARGET_ID="\*"
UNIT_TEST_CONNECT_RETRY_COUNT=10

# NOTE(ruel): receive argument and organize
while getopts ":f:m:p:c:vp:" opt; do
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

set -o monitor
PIDS=""
for DEVICE_ID in $DEVICE_IDS;
do
	bash launch_apk.sh $* -i $DEVICE_ID | awk -v DID="${DEVICE_ID:0:6}" '{print "["DID"] "$0}'&
	PIDS+="$! "
done

for PID in $PIDS;
do
	wait $PID
	if [ $? -eq 0 ];
	then
		echo "Succeeded: $PID done with $?"
	else 
		echo "Failed: $PID done with $?"
	fi 
done 

set +o monitor 
safe_exit 0
