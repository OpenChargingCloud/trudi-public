# IF_Adapter_TRuDI

Ansprechpartner:

	Thomas Müller
	IVU Softwareentwicklung GmbH
	tmueller@ivugmbh.de
	Tel.: 09471 / 30 73 237  

## Entwicklungsumgebung

- Visual Studio 2017 Preview: https://www.visualstudio.com/de/vs/preview
- .Net Core SDK 2.0 Preview: https://www.microsoft.com/net/core/preview
- Alternativ zu Visual Studio kann auch Visual Studio Code verwendet werden: https://code.visualstudio.com/

## Namenskonvention

Assemblies müssen folgendermaßen benannt werden, z.B.:

```csharp
TRuDI.HanAdapter.Example
```

Wobei "Example" durch den Namen des Gateway-Herstellers zu ersetzten ist. 

Die Klasse, welche das Interface IHanAdapter impelmentiert, muss wie folgt benannt werden:

```csharp
namespace TRuDI.HanAdapter.Example
{
   public class HanAdapterExample : IHanAdapter
   {
      // ...
   }
}
```

## Einbinden von IHanAdapter

Das Interface muss über das NuGet-Paket "TRuDI.HanAdapter.Interface.1.0.0.nupkg" in das jeweilige Projekt eingebunden werden.
Im Repository ist ebenfalls das zugehörige Projekt zu finden. Dieses dient allerdings nur zur Dokumentation der Schnittstelle und sollte nicht direkt verwendet werden.

## HTTPS

Für den Zugriff auf die Gateways über HTTPS stellt die IVU Softwareentwicklung eine TLS-Bibliothek sowie einen HTTP-Client zur verfügung.
Diese Komponenten gewährleisten die geforderte TLS-Funktionalität unter allen zu unterstüzenden Plattformen. 

Es werden folgende Cipher Suites unterstützt:
- TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256
- TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384
- TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256
- TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384

Elliptische Kurven:
- Brainpool P256r1
- Brainpool P384r1
- Brainpool P512r1
- NIST P-256
- NIST P-384

Die HTTP-Client-Klasse basiert auf der Version aus den .Net Core Foundation Libraries: https://msdn.microsoft.com/en-us/library/system.net.http.httpclient

Die NuGet-Pakete sind im Verzeichnis "private-packages" zu funden. Dieses Verzeichnis ist in den Beispiel-Projekten auch als Paket-Quelle angegeben.

## Logging

Zum Logging innerhalb des Adapters empfehlen wir LibLog zu verwenden: https://github.com/damianh/LibLog 

Die Ausgeben der Log-Meldungen übernimmt dadurch der Logger im aufrufenden Programm. Im Beispiel ist dies Serilog (https://serilog.net/)

## Build der Beispiele

Im Verzeichnis ``src\TRuDI.HanAdapter.Test`` folgende Befehle ausführen

- ``dotnet restore``

	Stellt die in der Projekt-Datei angegebenen Abhänigkeiten wieder her (z.B. Download von NuGet-Paketen).

- ``dotnet build``

	Test-Applikation sowie den Beispiel-Adapter erstellen.

- ``dotnet run``

	Test-Applikation ausführen.


## Ablauf

### Grundsätzliches

Methoden, welche länger als 3 Sekunden zur Ausführung benötigen, sollten über den Callback ``Action<ProgressInfo> progressCallback`` den 
Benutzer über den aktuellen Fortschritt der jeweiligen Operation informieren. 

Über das ``CancellationToken`` ist es dem Benutzer möglich, die aktuelle Operation jederzeit abzubrechen.

### 1. Verbindungsaufbau zum Gateway mittels ``Connect``

### 2. Laden der zum Kunden gehörenden Verträge mittels ``LoadAvailableContracts``

### 3. Laden der Daten zum vom Kunden ausgewählten Vertrag mittels ``LoadData``


