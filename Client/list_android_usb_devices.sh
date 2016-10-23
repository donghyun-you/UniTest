#!/bin/bash
adb devices | grep -E '^[0-9a-zA-Z]{8,16}' | awk '{print $1}'
