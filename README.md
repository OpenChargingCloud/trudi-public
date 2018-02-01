# TRuDI - Transparenz- und Display-Software

Ansprechpartner:

	Thomas Müller
	IVU Softwareentwicklung GmbH
	tmueller@ivugmbh.de

## Entwicklungsumgebung

### Microsoft .Net Core

- Visual Studio 2017 15.4: https://www.visualstudio.com/de/vs/
- .Net Core SDK 2.1.2: https://www.microsoft.com/net/core/
- Alternativ zu Visual Studio kann auch Visual Studio Code verwendet werden: https://code.visualstudio.com/

### Electron

- Node.js 8.9: https://nodejs.org/

### Build

Sind das .Net Core SDK sowie Node.js installiert, können die folgenden Build-Skripte zum erstellen der Installations-Pakete verwendet werden.

#### Windows: src/build.cmd 
  
Erzeugt jeweils ein Installations-Paket für 32- und 64-Bit-Windows-System ab Windows 7.

#### Linux: src/build.sh

Erzeugt ein AppImage sowie ein DEB-Software-Paket.

## Installations-Pakete

Die aktuelle Version von TRuDI ist 1.0.38.

System           | Download
---              | ---
Windows (64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-Setup-1.0.38-x86_64.exe
Windows (32-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-Setup-1.0.38-x86_32.exe
Linux ([AppImage](https://de.wikipedia.org/wiki/AppImage), 64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-1.0.38-x86_64.AppImage
Linux (deb, 64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-1.0.38_amd64.deb
Linux Live-Image | http://www.ivu-software.de/apps/TRuDI/TRuDI_1.0.37_live.iso (Testversion!)

## Dokumentation

[HAN-Adapter](doc/han-adapter.md)

[TAF-Adapter](doc/taf-adapter.md)

[Architektur-Dokumentation](doc/architecture-documentation.md)

[Erstellen eines Live Linux ISO-Image](doc/linux-live-image.md)


## Programm-Argumente von TRuDI

#### ``-l|--log <logfile>``

Datei in der Programmmeldungen protokolliert werden.

#### ``--loglevel <loglevel>``

Log Level: ``verbose``, ``debug``, ``info``, ``warning``, ``error``, ``fatal``. Standard ist ``info``.

#### ``-t|--test <testconfig>``

Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.


## Starten von TRuDI aus Visual Studio

Um TRuDI aus Visual Studio heraus zu starten, sind folgende Einstellungen auf der Seite Debug des Projekts TRuDI.Backend nötig:

![Debug-Einstellungen](doc/Images/Debug-Settings_VS2017.png)

- Launch: Project
- App URL: http://localhost:5000/

Wird TRuDI.Backend mit der Debug-Konfiguration kompiliert und gestartet, so wird immer der Port 5000 für die Webseite verwendet.
