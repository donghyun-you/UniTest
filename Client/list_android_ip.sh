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

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

if [ ! -v DEVICE_ID ]
then 
	echo "Error: -i option (DEVICE_ID, ip or adb usb identifier) required" >&2
	exit 1;
fi

LOCAL_IPS_HEAD=$(ifconfig | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
DEVICE_IP_ADDRS=$(adb -s $DEVICE_ID shell ip addr show | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}')
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

echo $RESULT | tr ' ' '\n' | grep -v ^0. | grep -v ^127.

if [ "$VERBOSE" == "TRUE" ];
then
	set +x;
fi
