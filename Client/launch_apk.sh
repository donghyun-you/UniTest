#!/usr/bin/bash

adb install test.apk
adb shell am start -n com.ruel.unitest/com.unity3d.player.UnityPlayerActivity
