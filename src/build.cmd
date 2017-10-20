rem TRuDI Build Script
rem 

rem Build 64-Bit version 

rem Build ASP.NET Core backend
cd TRuDI.Backend
rmdir /S /Q bin\dist
rmdir /S /Q ..\..\dist\win-unpacked

dotnet build -c Release
dotnet publish -c Release -r win7-x64 --self-contained -o bin\dist\win7-x64 -p:SelfContainedBuild=true
del /S bin\dist\*.pdb

rem Copy precompiled Views to self-contained output
copy bin\Release\netcoreapp2.0\TRuDI.Backend.PrecompiledViews.dll bin\dist\win7-x64\TRuDI.Backend.PrecompiledViews.dll

rem Build Electron frontend
cd ..\TRuDI.Frontend

rem Generate checksums file
node ..\Utils\createDigestList.js ..\TRuDI.Backend\bin\dist\win7-x64 checksums-win32-x64.json

rem Build Electron Setup
call node_modules\.bin\electron-builder.cmd --x64


rem Build 32-Bit version

rem Build ASP.NET Core backend
cd ..\TRuDI.Backend
rmdir /S /Q bin\dist
rmdir /S /Q ..\..\dist\win-unpacked

dotnet build -c Release
dotnet publish -c Release -r win7-x86 --self-contained -o bin\dist\win7-x86 -p:SelfContainedBuild=true
del /S bin\dist\*.pdb

rem Copy precompiled Views to self-contained output
copy bin\Release\netcoreapp2.0\TRuDI.Backend.PrecompiledViews.dll bin\dist\win7-x86\TRuDI.Backend.PrecompiledViews.dll

rem Build Electron frontend
cd ..\TRuDI.Frontend

rem Generate checksums file
node ..\Utils\createDigestList.js ..\TRuDI.Backend\bin\dist\win7-x86 checksums-win32-x86.json

rem Build Electron Setup
call node_modules\.bin\electron-builder.cmd --ia32