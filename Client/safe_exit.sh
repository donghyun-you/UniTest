#!/bin/bash

function safe_exit(){

	if [ "$VERBOSE" == "TRUE" ];
	then
		set +x;
	fi

	exit $1
}
