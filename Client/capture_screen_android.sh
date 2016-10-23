#!/bin/bash

# NOTE(ruel): receive argument and organize
while getopts ":vi:x:" opt; do
	case $opt in
		:)
			echo "Usage: -i {device id(ip,usb id} -v(verbose) -x {export path}"
			exit 1
			;;
		v)
			VERBOSE="TRUE"
			;;
		i)
			DEVICE_ID=$OPTARG
			;;
		x)
			EXPORT=$OPTARG
			;;
	esac
done

source ./safe_exit.sh

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

if [ ! -v DEVICE_ID ]
then 
	echo "Error: -i option (DEVICE_ID, ip or adb usb identifier) required" 1>&2
	safe_exit 1
fi

SCREENSHOT=$(echo "/tmp/"$DEVICE_ID".screen.png")
SCREENSHOT_REMOTE_TEMP=$(echo "/sdcard/"$DEVICE_ID".screen.png")

if [ -v EXPORT ];
then 
	SCREENSHOT=$EXPORT
fi

echo "Export($DEVICE_ID) screenshot into $SCREENSHOT"

adb -s $DEVICE_ID shell screencap -p $SCREENSHOT_REMOTE_TEMP
adb -s $DEVICE_ID pull $SCREENSHOT_REMOTE_TEMP $SCREENSHOT
adb -s $DEVICE_ID shell rm $SCREENSHOT_REMOTE_TEMP

safe_exit 0
