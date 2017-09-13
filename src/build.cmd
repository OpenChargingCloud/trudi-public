rem TRuDI Build Script
rem 

rem Build ASP.NET Core backend
cd TRuDI.Backend
rmdir /S /Q bin\dist
rmdir /S /Q ..\..\dist\win-unpacked

dotnet publish -c Release -r win7-x64 --self-contained -o bin\dist\win7-x64
del /S bin\dist\*.pdb

rem Build Electron frontend
cd ..\TRuDI.Frontend

rem Generate checksums file
node ..\Utils\createDigestList.js ..\TRuDI.Backend\bin\dist\win7-x64 checksums-win32-x64.json

rem Build Electron Setup
npm run dist




rem Install electron:
rem npm install
rem npm install electron-builder --save-dev
