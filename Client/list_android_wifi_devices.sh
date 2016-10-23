#!/bin/bash
adb devices | grep -E '^[0-9\.]{8,16}' | awk '{print $1}'
