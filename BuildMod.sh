#!/bin/bash

err(){
    echo "There are compiling errors, exiting..."
    exit
};

echo "Compiling..."
cd src
msbuild -m /verbosity:minimal /p:Configuration=Developer || err
cd ..
cp "CompileFiles/bin/Developer/ILTG.dll" Packing/

echo "Packing..."
cd Packing
zip -q -r -0 "ILTG Shader.zip" * -x Assets/ILTG/*
mv "ILTG Shader.zip" "../../../Mods/ILTG Shader.scmod"
cd ..

echo -e "\033[32mDone, you can open the game now\033[0m"