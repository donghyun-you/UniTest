#!/usr/bin/bash

source ./dependency_ios.sh

# NOTE(ruel): default as usb devices
VERBOSE="FALSE"
DEPLOY_TARGET_ID="\*"
UNIT_TEST_CONNECT_RETRY_COUNT=10

# NOTE(ruel): receive argument and organize
while getopts ":f:p:v" opt; do
	case $opt in
		:)
			echo "Usage: -f {IOS_APP_PATH} -i {DEPLOY_IOS_DEVICE_TARGET_ID} -v(verbose)"
			exit 1
			;;
		v)
			VERBOSE="TRUE"
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


set -o monitor
PIDS=""
for DEVICE_ID in $DEVICE_IDS;
do 
	bash launch_ios_app.sh $* -i $DEVICE_ID | awk -v DID="${DEVICE_ID:0:6}" '{print "["DID"] "$0}'&
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
