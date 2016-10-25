#!/usr/bin/bash

source ./dependency_ios.sh

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
DEPLOY_TARGET_ID="\*"
UNIT_TEST_CONNECT_RETRY_COUNT=10

# NOTE(ruel): receive argument and organize
while getopts ":f:p:vi:" opt; do
	case $opt in
		:)
			echo "Usage: -f {IOS_APP_PATH} -i {DEPLOY_IOS_DEVICE_TARGET_ID} -v(verbose)"
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
			
	esac
done

source ./safe_exit.sh

if [ "$VERBOSE" == "TRUE" ];
then
	set -x;
fi

DEVICE_IDS=$(ios-deploy --detect | grep -Eo "[a-fA-F0-9]{40}")

# NOTE(ruel): check argument configured
if [ ! -v IOS_APP ]; 
then 
	echo "Error: -f {IOS_APP_PATH} option required" 1>&2
	safe_exit 1
fi

# NOTE(ruel): check file exists
if [ ! -d $IOS_APP ];
then	
	echo $IOS_APP" is not exists!" 1>&2
	safe_exit 1
fi

BUNDLE_ID=$(mdls -name kMDItemCFBundleIdentifier -r $IOS_APP)

if [ -z "$BUNDLE_ID" ]
then
	echo $IOS_APP" is not ios app. unable to retrieve bundle id" 1>&2
	safe_exit 1
fi 

echo "iOS App file to install: $IOS_APP, Bundle Identifier: $BUNDLE_ID"

for DEVICE_ID in $DEVICE_IDS;
do
	if [ $DEPLOY_TARGET_ID == "\*" ] || [ $DEVICE_ID == $DEPLOY_TARGET_ID ]
	then 
		echo "Try uninstalling $BUNDLE_ID ..."
		ios-deploy --id $DEVICE_ID --no-wifi --uninstall_only --bundle_id $BUNDLE_ID

		echo "Installing and launching $BUNDLE_ID from $IOS_APP ..."
		LAUNCH_RESULT=$(ios-deploy --id $DEVICE_ID --no-wifi --justlaunch --bundle $IOS_APP)

		IFSB=$IFS
		IFS=$'\n'
		for LINE in $LAUNCH_RESULT
		do 
			echo $LINE
		done 
		IFS=$IFSB

		if [ ! -z "$(echo $LAUNCH_RESULT | grep -e 'Device Locked')" ]
		then
			echo "$DEVICE_ID is device locked. skipping test over wifi"
		else
			IP_ADDRS_GET_RESULT=1

			i=$UNIT_TEST_CONNECT_RETRY_COUNT # try UNIT_TEST_CONNECT_RETRY_COUNT times for each app launch
			while [ $IP_ADDRS_GET_RESULT -eq 1 ];
			do
				IP_ADDRS=$(source ./list_ios_ip.sh -i $DEVICE_ID -b $BUNDLE_ID ) </dev/null; IP_ADDRS_GET_RESULT=$?
				sleep 1

				if [ $((i--)) -lt 0 ]
				then
					echo "Error: Out of retry count(retrieve ip file)" 1>&2
					safe_exit 1
				fi 

			done

			echo "ip detected: $IP_ADDRS"

			if [ -z "$IP_ADDRS" ]
			then 
				echo "unable to detect any matched ip to connect unit test servers"
			else 
				# try connect to unit test server with each ips
				i=$UNIT_TEST_CONNECT_RETRY_COUNT # try UNIT_TEST_CONNECT_RETRY_COUNT times for each IP_ADDR
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


	fi 
done

safe_exit 0
