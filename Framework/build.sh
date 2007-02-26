#!/bin/bash

CURDIR=$(pwd)
cd $(dirname $0)
BUILDDIR=$(pwd)

MONO_EXE=$(which gmcs)

if [ ! -x $MONO_EXE ]
then
	echo "Can't find mono.exe"
	exit 5
fi

echo
echo Building UvsChess 
echo

resources=
cd Images
for curImg in $(ls *.png)
do
	resources="$resources -resource:Images/$curImg,UvsChess.Images.$curImg"
done
cd ..

$MONO_EXE -t:exe -out:UvsChess.exe ./*.cs -pkg:gtk-sharp-2.0 $resources

if [ $? -eq 0 ]
then

	echo "Build complete"
else
	echo "Build failed"
fi
echo
