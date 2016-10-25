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

if [ -z "$(which sort)" ]
then 
	echo "Error: Unable to find sort. make alias" 1>&2
	exit
fi 

if [ -z "$(which head)" ]
then 
	echo "Error: Unable to find head. make alias" 1>&2
	exit 
fi

if [ -z "$(which awk)" ]
then 
	echo "Error: Unable to find awk. make alias" 1>&2
	exit
fi 
