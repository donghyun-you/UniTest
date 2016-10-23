#!/bin/bash

if [ -z "$(which ios-deploy)" ]
then
	echo "Error: Unable to find ios-deploy. make alias" 1>&2
	exit 1
fi

if [ -z "$(which nc)" ]
then
	echo "Error: Unable to find nc. make alias" 1>&2
	exit 1
fi

if [ -z "$(which mdls)" ]
then
	echo "Error: Unable to find mdls. make alias" 1>&2
	exit 1
fi
