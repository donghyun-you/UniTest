#!/usr/bin/bash

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
PACKAGE="com.ruel.unitest"
DEPLOY_TARGET_ID="\*"

# NOTE(ruel): receive argument and organize
while getopts ":f:p:vp:i:" opt; do
	case $opt in
		:)
			echo "Usage: -f {IOS_APP_PATH}, -p [Package name:default(com.ruel.unitest)] -v(verbose)"
			exit 1
			;;
		v)
			VERBOSE="TRUE"
			;;
		i)
			DEPLOY_TARGET_ID=$OPTARG
			;;
		f)
			IOS_APP=$OPTARG
			;;
		p)
			PACKAGE=$OPTARG
			;;
			
	esac
done

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

#TEMP_FOLDER="/tmp/"$(date +%s)"_"$IOS_APP

#LOCAL_IPS_HEAD=$(ifconfig | grep inet | awk '{print $2}' | grep -Eo '^[0-9\.]{8,16}' | grep -v ^127 |awk '{split($0,a,"."); print a[1]"."a[2]"."a[3]}')
DEVICE_IDS=$(ios-deploy --detect | grep -Eo "[a-fA-F0-9]{40}")

# NOTE(ruel): check argument configured
if [ ! -v IOS_APP ]; 
then 
	echo "Error: -f {IOS_APP_PATH} option required";
	exit 0;
fi

# NOTE(ruel): check file exists
if [ ! -d $IOS_APP ];
then	
	echo $IOS_APP" is not exists!"
	exit 0;
fi

echo "iOS App file to install: $IOS_APP"

for DEVICE_ID in $DEVICE_IDS;
do
	if [ $DEPLOY_TARGET_ID == "\*" ] || [ $DEVICE_ID == $DEPLOY_TARGET_ID ]
	then 
		echo "Try uninstalling $PACKAGE ..."
		ios-deploy --id $DEVICE_ID --no-wifi --uninstall_only --bundle_id $PACKAGE

		echo "Installing and launching $PACKAGE from $IOS_APP ..."
		ios-deploy --id $DEVICE_ID --no-wifi --justlaunch --bundle $IOS_APP

		IP_ADDRS_GET_RESULT=1

		while [ $IP_ADDRS_GET_RESULT -eq 1 ];
		do
			IP_ADDRS=$(source ./list_ios_ip.sh -i $DEVICE_ID -b com.ruel.unitest ) </dev/null; IP_ADDRS_GET_RESULT=$?
			sleep 1
		done

		if [ -z "$IP_ADDRS" ]
		then 
			echo "unable to detect any matched ip to connect unit test servers"
		else 
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

if [ "$VERBOSE" == "TRUE" ];
then
	set +x;
fi
