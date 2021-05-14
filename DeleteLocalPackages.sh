#!/bin/bash

export PACKAGEVERSION=1.1000.0
export TARGETROOT=~/.nuget/packages

find $TARGETROOT/ -name $PACKAGEVERSION -exec rm -rf {} \;