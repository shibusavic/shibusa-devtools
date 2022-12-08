#!/bin/bash

start_dir=$(pwd)
src_dir=$1
target_dir=$2

cp "${target_dir}/.config" ./.config-copy

cd $target_dir
rm -rf *

cd $start_dir

cd $src_dir

cd Shibusa.DevTools.Cli
dotnet build -c Release -o "${target_dir}/"

cd ../Shibusa.DevTools.Config.Cli
dotnet build -c Release -o "${target_dir}/config/"

cd ../Shibusa.DevTools.FindLines.Cli
dotnet build -c Release -o "${target_dir}/find-lines/"

cd ../Shibusa.DevTools.CsProjects.Cli
dotnet build -c Release -o "${target_dir}/cs-projects/"

cd ../Shibusa.DevTools.CsGenerator.Cli
dotnet build -c Release -o "${target_dir}/cs-gen/"

cd $start_dir
mv ./.config-copy "${target_dir}/.config"