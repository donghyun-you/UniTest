#!/usr/bin/bash
ADB_EN_ADDR=$(adb devices | grep ^[0-9] | awk '{print $1}' | awk '{split($0,a,":"); print a[1]}')

APK=$1
set -x
for ADDR in $ADB_EN_ADDR;
do
	SCREENSHOT=$(echo "/tmp/"$ADDR".screen.png")
	SCREENSHOT_REMOTE_TEMP=$(echo "/sdcard/"$ADDR".screen.png")
	adb -s $ADDR uninstall com.ruel.unitest
	adb -s $ADDR install "$APK"

	adb -s $ADDR shell input keyevent 26 #Pressing the lock button
	adb -s $ADDR shell input keyevent 66 #Pressing Enter

	adb -s $ADDR shell am start -n com.ruel.unitest/com.unity3d.player.UnityPlayerActivity
	sleep 5
	python client.py -a $ADDR -p 7701
	
	adb -s $ADDR shell screencap -p $SCREENSHOT_REMOTE_TEMP
	adb -s $ADDR pull $SCREENSHOT_REMOTE_TEMP $SCREENSHOT
	adb -s $ADDR shell rm $SCREENSHOT_REMOTE_TEMP
	open $SCREENSHOT
done
set +x
