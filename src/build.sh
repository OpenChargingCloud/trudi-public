#!/bin/sh
# TRuDI Build Script
#
# Required packages to build on Ubuntu:
#
#   sudo apt-get install --no-install-recommends -y icnsutils graphicsmagick xz-utils
#   sudo apt-get install xorriso -y
#   sudo npm install electron-builder -g


cd TRuDI.Backend
rm -rf bin/dist
rm -rf ../../dist/linux-unpacked

dotnet build -c Release
dotnet publish -c Release -r linux-x64 --self-contained -o bin/dist/linux-x64 -p:SelfContainedBuild=true

# Copy precompiled Views to self-contained output
cp bin/Release/netcoreapp2.0/TRuDI.Backend.PrecompiledViews.dll bin/dist/linux-x64/TRuDI.Backend.PrecompiledViews.dll

# Build Electron frontend
cd ../TRuDI.Frontend

# Generate checksums file
node ../Utils/createDigestList.js ../TRuDI.Backend/bin/dist/linux-x64 checksums-linux.json

USE_SYSTEM_XORRISO=true npm run dist


