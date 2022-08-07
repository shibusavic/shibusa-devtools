#!/bin/bash

start_dir=$(pwd)
src_dir=$1
target_dir=$2

cd $src_dir
cd Shibusa.DevTools.Cli
dotnet build -c Release -o "${target_dir}/"

cd ../Shibusa.DevTools.FindText.Cli
dotnet build -c Release -o "${target_dir}/find-text/"

cd ../Shibusa.DevTools.CsProjects.Cli
dotnet build -c Release -o "${target_dir}/cs-projects/"

cd $start_dir