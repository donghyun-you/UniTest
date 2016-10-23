#!/bin/bash

if [ -z "$(which adb)" ]
then
	echo "Error: Unable to find adb. install adb, and make alias" 1>&2
	exit 1
fi

if [ -z "$(which nc)" ]
then
	echo "Error: Unable to find nc. make alias" 1>&2
	exit 1
fi

if [ -z "$(which aapt)" ]
then
	echo "Error: Unable to find aapt. make alias" 1>&2
	exit 1
fi
