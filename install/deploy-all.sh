#!/bin/bash

start_dir=$(pwd)
src_dir=$1
target_dir=$2

cd $src_dir
cd Shibusa.DevTools.Cli
dotnet build -c Release -o "${target_dir}/"

cd ../Shibusa.DevTools.FindLines.Cli
dotnet build -c Release -o "${target_dir}/find-lines/"

cd ../Shibusa.DevTools.CsProjects.Cli
dotnet build -c Release -o "${target_dir}/cs-projects/"

cd $start_dir