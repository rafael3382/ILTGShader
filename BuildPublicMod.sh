#!/bin/bash

err(){
    echo "There are compiling errors, exiting..."
    exit
};


echo "Compiling..."
cd src
msbuild -m /p:Configuration=Release || err
cd ..
cp "CompileFiles/bin/Release/ILTG.dll" Packing/

echo "Packing..."
cd Packing
zip -5 -r "ILTG Shader.zip" * > /dev/null
mv "ILTG Shader.zip" "../../../Mods/ILTG Shader.scmod"
cd ..