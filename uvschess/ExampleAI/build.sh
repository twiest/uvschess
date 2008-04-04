#!/bin/bash

# This script show how to write your own script to compile your AI dll with Mono.

CURDIR=$(pwd)
cd $(dirname $0)
BUILDDIR=$(pwd)

# The name of your AI dll
AI_NAME="ExampleAI"

# Check that Mono is installed
MONO_EXE=$(which gmcs)
if [ ! -x $MONO_EXE ]
then
	echo "Can't find mono.exe"
	exit 5
fi

echo
echo Building $AI_NAME 
echo

# Compile the project into a dll. See the man pages for gmcs for info about the options
$MONO_EXE -t:library -out:$AI_NAME.dll -langversion:linq ./*.cs -r:../Framework/UvsChess.exe 

if [ $? -ne 0 ]
then
	echo "Build failed"
	exit 5
fi

echo "Build complete"

# If the build was successful, copy it to the dir where UvsChess.exe is
cp $AI_NAME.dll ../Framework/
echo
