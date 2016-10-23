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

function safe_exit(){
	
	if [ "$VERBOSE" == "TRUE" ];
	then
		set +x;
	fi

	exit $1
}

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

if [ ! -v DEVICE_ID ]
then 
	echo "Error: -i option (DEVICE_ID, ip or adb usb identifier) required" >&2
	safe_exit 1
fi

TEMP_FOLDER="/tmp/"$(date +%s)"_"$IOS_APP
IP_ADDRS_FILE=$TEMP_FOLDER"/Documents/ip_address"

if [ -f $IP_ADDR_FILE ];
then 
	rm $IP_ADDRS_FILE
fi 

ios-deploy --id $DEVICE_ID --no-wifi --bundle_id com.ruel.unitest --download=/Documents/ip_address --to $TEMP_FOLDER

if [ ! -f $IP_ADDRS_FILE ];
then
	echo "Error: unable to download ip_address from $DEVICE_ID yet"
	safe_exit 1
fi

LOCAL_IPS_HEAD=$(ifconfig | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}' | grep -v ^127)
DEVICE_IP_ADDRS=$(cat $IP_ADDRS_FILE)
DEVICE_IP_ADDRS_HEAD=$(echo $DEVICE_IP_ADDRS | awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
IP_ADDRS_INTERSECT=$(echo $LOCAL_IPS_HEAD $DEVICE_IP_ADDRS_HEAD | tr ' ' '\n' | sort | uniq -d)

for IP_ADDR_INTERSECT in $IP_ADDRS_INTERSECT;
do
	SCOPE_RESULT=$(echo $DEVICE_IP_ADDRS | grep $IP_ADDR_INTERSECT)

	if [ -v RESULT ];
	then
		RESULT=$RESULT" "$SCOPE_RESULT
	else
		RESULT=$SCOPE_RESULT
	fi
done

echo $RESULT | tr ' ' '\n'

safe_exit 0
