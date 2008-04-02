#!/bin/bash

#This builds both the UvsChess framework and the ExampleAI


./Framework/build.sh
if [ $? -ne 0 ]
then
	exit 5
fi	


./ExampleAI/build.sh
if [ $? -ne 0 ]
then
	exit 5
fi	



