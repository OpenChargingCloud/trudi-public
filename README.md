# IF_Adapter_TRuDI

Ansprechpartner:

	Thomas M�ller
	IVU Softwareentwicklung GmbH
	tmueller@ivugmbh.de
	Tel.: 09471 / 30 73 237  

## Entwicklungsumgebung

- Visual Studio 2017 Preview: https://www.visualstudio.com/de/vs/preview
- .Net Core SDK 2.0 Preview: https://www.microsoft.com/net/core/preview
- Alternativ zu Visual Studio kann auch Visual Studio Code verwendet werden: https://code.visualstudio.com/

## Namenskonvention

Assemblies m�ssen folgenderma�en benannt werden, z.B.:

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

Das Interface muss �ber das NuGet-Paket "TRuDI.HanAdapter.Interface.1.0.0.nupkg" in das jeweilige Projekt eingebunden werden.
Im Repository ist ebenfalls das zugeh�rige Projekt zu finden. Dieses dient allerdings nur zur Dokumentation der Schnittstelle und sollte nicht direkt verwendet werden.

## HTTPS

F�r den Zugriff auf die Gateways �ber HTTPS stellt die IVU Softwareentwicklung eine TLS-Bibliothek sowie einen HTTP-Client zur verf�gung.
Diese Komponenten gew�hrleisten die geforderte TLS-Funktionalit�t unter allen zu unterst�zenden Plattformen. 

Es werden folgende Cipher Suites unterst�tzt:
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

Die Ausgeben der Log-Meldungen �bernimmt dadurch der Logger im aufrufenden Programm. Im Beispiel ist dies Serilog (https://serilog.net/)

## Build der Beispiele

Im Verzeichnis ``src\TRuDI.HanAdapter.Test`` folgende Befehle ausf�hren

- ``dotnet restore``

	Stellt die in der Projekt-Datei angegebenen Abh�nigkeiten wieder her (z.B. Download von NuGet-Paketen).

- ``dotnet build``

	Test-Applikation sowie den Beispiel-Adapter erstellen.

- ``dotnet run``

	Test-Applikation ausf�hren.


## Ablauf

### Grunds�tzliches

Methoden, welche l�nger als 3 Sekunden zur Ausf�hrung ben�tigen, sollten �ber den Callback ``Action<ProgressInfo> progressCallback`` den 
Benutzer �ber den aktuellen Fortschritt der jeweiligen Operation informieren. 

�ber das ``CancellationToken`` ist es dem Benutzer m�glich, die aktuelle Operation jederzeit abzubrechen.

### 1. Verbindungsaufbau zum Gateway mittels ``Connect``

### 2. Laden der zum Kunden geh�renden Vertr�ge mittels ``LoadAvailableContracts``

### 3. Laden der Daten zum vom Kunden ausgew�hlten Vertrag mittels ``LoadData``


