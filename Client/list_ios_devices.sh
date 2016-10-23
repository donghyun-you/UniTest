##!/bin/bash
ios-deploy --detect | grep -Eo "[a-fA-F0-9]{40}"
