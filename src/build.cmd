cd TRuDI.Backend
rmdir /S /Q bin\dist
dotnet publish -c Release -r win7-x64 --self-contained -o bin\dist\win7-x64
cd ..\TRuDI.Frontend
npm run dist

rem Install electron:
rem npm install
rem npm install electron-builder --save-dev

