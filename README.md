# TRuDI - Transparenz- und Display-Software

Ansprechpartner:

	Thomas Müller
	IVU Softwareentwicklung GmbH
	tmueller@ivugmbh.de
	Tel.: 09471 / 30 73 237  

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

## HAN-Adapter

## Einbinden eines neuen HAN-Adapters

In der Klasse (Projekt TRuDI.HanAdapter.Repository)

```csharp
TRuDI.HanAdapter.Repository.HanAdapterRepository
```

muss der neue HAN-Adapter in die Liste ```availableAdapters``` eingetragen werden.

Z.B.: 

```csharp
new HanAdapterInfo("XXX", "Beispiel GmbH", typeof(HanAdapterBeispiel)),
```

Während der Entwicklung des HAN-Adapters kann dieser als Projekt-Referenz in das Projekt TRuDI.HanAdapter.Repository aufgenommen werden.

Die spätere Integration erfolt als NuGet-Paket. Beispiel für die entsprechenden Projekt-Einstellungen:
![Projekt-Einstellungen HAN-Adapter](doc/Images/HAN-Adapter_VS2017_Package_Settings.png)

Die NuGet-Pakete der HAN-Adapter werden im Verzeichnis private-packages abgelegt.

## Namenskonvention HAN-Adapter

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

Das Interface ist im Projekt ```TRuDI.HanAdapter.Interface``` zu finden.

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

Die Ausgabe der Log-Meldungen übernimmt dadurch der Logger im aufrufenden Programm. In TRuDI wird hierzu Serilog (https://serilog.net/) verwendet.

## HAN-Adapter-Test-Programm

Das Projekt TRuDI.HanAdapter.Test dient als Test-Programm für die HAN-Adapter. Damit können diese über die Kommandozeile aufgerufen werden.

```
TRuDI HAN Adapter Test Application

Usage: TRuDI.HanAdapter.Test [options] [command]

Options:
  -?|-h|--help               Show help information
  -l|--log <log-file>        Logmeldungen werden in die angegebene Datei geschrieben.
  --log-console              Logmeldungen werden auf der Konsole ausgegeben.
  --loglevel <log-level>     Log Level: verbose, debug, info, warning, error, fatal. Standard ist info.
  -t|--test <test-config>    Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.
  -o|--output <output-file>  Ausgabedatei.

Commands:
  adapter-list       Generiert eine List mit allen bekannten HAN-Adaptern.
  contract-list      Liest die Liste der für den Benutzer verfügbaren Verträge aus dem SMGW.
  current-registers  Liest die aktuellen Registerwerte für den angegebenen Vertrag aus dem SMGW.
  load-data          Liest die Daten des angegebenen Vertrags aus dem SMGW.

Use "TRuDI.HanAdapter.Test [command] --help" for more information about a command.
```

```
Usage: TRuDI.HanAdapter.Test contract-list [options]

Options:
  --help|-h|-?         Show help information
  --user <username>    Benutzername
  --cert <cert-file>   PKCS#12-Datei mit Client-Zertifikat und dazugehörigen Key
  --pass <password>    Passwort zum Benutzernamen oder ggf. für die PKCS#12-Datei.
  --id <serverid>      Herstellerübergreifende ID des SMGW (z.B. "EABC0012345678")
  --addr <address>     IP-Adresse des SMGW.
  --port <port>        Port des SMGW.
  --timeout <timeout>  Timeout in Sekunden nachdem der Vorgang über das CancellationToken abgebrochen wird.
```

```
Usage: TRuDI.HanAdapter.Test load-data [options]

Options:
  --help|-h|-?                   Show help information
  --user <username>              Benutzername
  --cert <cert-file>             PKCS#12-Datei mit Client-Zertifikat und dazugehörigen Key
  --pass <password>              Passwort zum Benutzernamen oder ggf. für die PKCS#12-Datei.
  --id <serverid>                Herstellerübergreifende ID des SMGW (z.B. "EABC0012345678")
  --addr <address>               IP-Adresse des SMGW.
  --port <port>                  Port des SMGW.
  --timeout <timeout>            Timeout in Sekunden nachdem der Vorgang über das CancellationToken abgebrochen wird.
  --usagepointid <usagePointId>  Zählpunktsbezeichnung (optional)
  --tariffname <tariffName>      Identifikation des Tarifs
  --billingperiod <index>        Index der Abrechnungsperiode (bei TAF-7 nicht benötigt)
  --start <start>                Zeitstempel, formatiert nach ISO8601
  --end <end>                    Zeitstempel, formatiert nach ISO8601
  --skip-validation              XML-Validierung nicht durchführen
  --taf6                         TAF-6-Abrechnungsperiode verwenden (nicht bei TAF-7)
```

```
Usage: TRuDI.HanAdapter.Test current-registers [options]

Options:
  --help|-h|-?                   Show help information
  --user <username>              Benutzername
  --cert <cert-file>             PKCS#12-Datei mit Client-Zertifikat und dazugehörigen Key
  --pass <password>              Passwort zum Benutzernamen oder ggf. für die PKCS#12-Datei.
  --id <serverid>                Herstellerübergreifende ID des SMGW (z.B. "EABC0012345678")
  --addr <address>               IP-Adresse des SMGW.
  --port <port>                  Port des SMGW.
  --timeout <timeout>            Timeout in Sekunden nachdem der Vorgang über das CancellationToken abgebrochen wird.
  --usagepointid <usagePointId>  Zählpunktsbezeichnung (optional)
  --tariffname <tariffName>      Identifikation des Tarifs
  --skip-validation              XML-Validierung nicht durchführen
```

### Beispiele

#### Abruf der verfügbaren HAN-Adapter

Zeigt eine Liste der verfügbaren HAN-Adapter an und schreibt diese ebenfalls in die Datei export.xml:

```
dotnet TRuDI.HanAdapter.Test.dll --output export.xml adapter-list
```

#### Abruf der für einen Benutzer verfügbaren Vertragsdaten

Zeigt die für den angegebenen Benutzer verfügbaren Vertragsdaten an und schreibt diese ebenfalls in die Datei export.xml:

```
dotnet TRuDI.HanAdapter.Test.dll --output export.xml contract-list --user consumer --pass consumer --addr 1.2.3.4 --port 1234 --id EXXX0012345678
```

#### Datenabruf für eine TAF-6-Abrechnungsperiode

Schreibt eine XML-Datei nach AR 2418-6 in die Datei export.xml:

```
dotnet TRuDI.HanAdapter.Test.dll --output export.xml load-data --user consumer --pass consumer --addr 1.2.3.4 --port 1234 --id EXXX0012345678 --billingperiod 0 --tariffname taf-2-test --taf6
```

## Ablauf

### Grundsätzliches

Methoden, bei denen der Callback ``Action<ProgressInfo> progressCallback`` übergeben wird, sollte 
möglichst häufig den aktuellen Fortschritt darüber an das Hauptprogramm weiter geben. 

Über das ``CancellationToken`` ist es dem Benutzer möglich, die aktuelle Operation jederzeit abzubrechen. 
In diesem Fall ist es erlaubt, die jeweilige Methode über eine ``OperationCanceledException`` zu verlassen.

### Reihenfolge der Methoden-Aufrufe

1. ``Connect()``

    Wird nach eingabe der Verbindungsparameter durch den Verbraucher aufgerufen.

2. ``LoadAvailableContracts()``

    Unmittelbar nach ``Connect()`` erfolg das Laden der für den Verbraucher relevanten Verträge.

3. ``LoadData()``

    Die Daten des vom Verbraucher gewählten Vertrages werden abgerufen.

4. ``GetCurrentRegisterValues()``
 
    Die aktuellen Registerwerte zum gegebenen Vertrag ermitteln.  

5. ``Disconnect()``

    Wird aufgerufen, um die aktive Verbindung zu beenden. Z.B. wenn der Benutzer sich an einem anderen SMGW anmelden möchte.


### 1. Verbindungsaufbau zum Gateway mittels ``Connect``

#### Authentifizierung über ein Client-Zertifikat

Für die Authentifizierung über ein Client-Zertifikat muss dieses Zertifikat inklusive des privaten Schlüssels in Form einer PKCS#12-Datei (https://tools.ietf.org/html/rfc7292) vorliegen.

Eine PKCS#12-Datei kann mittels OpenSSL wie folgt erstellt werden:

```
openssl pkcs12 -export -in Zertifikat.crt -inkey Schluessel.key -out Zertifikat_mit_Schluessel.p12
```

Der Inhalt der PKCS12-Datei wird der ``Connect()``-Methode unverändert übergeben. 

#### Beispiel-Implementierung

```csharp
public async Task<(ConnectResult result, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            byte[] pkcs12Data,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
{
    this.logger.Info("Connecting to {0} using a client certificate", endpoint);

    this.baseUri = $"https://{endpoint.Address}:{endpoint.Port}/base/path/to/data";

    var clientHandler = new IVU.Http.HttpClientHandler
                            {
                                AutomaticDecompression = DecompressionMethods.GZip 
                            };
            
    // Load the client certificate
    clientHandler.ClientCertificate = new ClientCertificateWithKey(pkcs12Data, password);

    X509Certificate2 serverCert = null;
    clientHandler.ServerCertificateCustomValidationCallback += (message, cert, chain, policyErrors) =>
        {
            // Important: chain an policyErrors are currently not filled
            serverCert = new X509Certificate2(cert);

            // accept the server certificate and continue with TLS handshake
            return true;
        };

    // Create the HttpClient instance
    this.client = new IVU.Http.HttpClient(clientHandler);

    // Set headers common for all calls
    this.client.DefaultRequestHeaders.Add("SMGW-ID", deviceId);

    // If there's a header value that changes with every request, create a HttpRequestMessage...
    var req = new IVU.Http.HttpRequestMessage(HttpMethod.Get, this.baseUri + "/login");
    req.Headers.Add("Request-GUID", Guid.NewGuid().ToString());

    try
    {
        // ... and call client.SendAsync with it. Otherwise client.GetAsync() can also be used.
        var loginResult = await this.client.SendAsync(req, ct);
        if (!loginResult.IsSuccessStatusCode)
        {
            return (null, new AdapterError(ErrorType.AuthenticationFailed));
        }
    }
    catch (Exception ex)
    {
        this.logger.ErrorException("Connect failed to {0}", ex, endpoint);
        return (null, new AdapterError(ErrorType.AuthenticationFailed));
    }

    // Query firmware version...

    return (new ConnectResult(serverCert, new FirmwareVersion[]{ /* ... */ }) null);
}
```

#### Authentifizierung über Benutzername und Passwort

#### Beispiel-Implementierung mit HTTP Digest Access Authentication nach RFC 7616 

```csharp
public async Task<(ConnectResult result, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            string user,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
{
    this.logger.Info("Connecting to {0} using user/password authentication", endpoint);

    this.baseUri = $"https://{endpoint.Address}:{endpoint.Port}/base/path/to/data";

    var clientHandler = new IVU.Http.HttpClientHandler
                            {
                                AutomaticDecompression = DecompressionMethods.GZip
                            };

    X509Certificate2 serverCert = null;
    clientHandler.ServerCertificateCustomValidationCallback += (message, cert, chain, policyErrors) =>
        {
            // Important: chain an policyErrors are currently not filled
            serverCert = new X509Certificate2(cert);

            // accept the server certificate and continue with TLS handshake
            return true;
        };

    // This example gateway uses Digest Access Authentication: add the DigestAuthMessageHandler to the client handler chain:
    var digestAuthMessageHandler = new DigestAuthMessageHandler(clientHandler, user, password);

    // Create the HttpClient instance
    this.client = new IVU.Http.HttpClient(digestAuthMessageHandler);

    // Set headers common for all calls
    this.client.DefaultRequestHeaders.Add("SMGW-ID", deviceId);

    // If there's a header value that changes with every request, create a HttpRequestMessage...
    var req = new IVU.Http.HttpRequestMessage(HttpMethod.Get, this.baseUri + "/login");
    req.Headers.Add("Request-GUID", Guid.NewGuid().ToString());

    try
    {
        // ... and call client.SendAsync with it. Otherwise client.GetAsync() can also be used.
        var loginResult = await this.client.SendAsync(req, ct);
        if (!loginResult.IsSuccessStatusCode)
        {
            return (null, new AdapterError(ErrorType.AuthenticationFailed));
        }
    }
    catch (Exception ex)
    {
        this.logger.ErrorException("Connect failed to {0}", ex, endpoint);
        return (null, new AdapterError(ErrorType.AuthenticationFailed));
    }

    // Query firmware version...

    return (new ConnectResult(serverCert, new FirmwareVersion[]{ /* ... */ }) null);
}
```

### 2. Laden der zum Verbraucher gehörenden Verträge mittels ``LoadAvailableContracts``

Direkt nach dem Verbindungsaufbau wird ``LoadAvailableContracts`` aufgerufen um eine Liste der für den 
Verbraucher relevanten Verträge zu erhalten.

``LoadAvailableContracts`` liefert eine Liste mit Instanzen der Klasse ``ContractInfo`` zurück.

Feld | Beschreibung|XML|Beispiel
---  | --- | --- | ---
TafId| Nummer des TAF | | TAF-1, TAF-2, TAF-6, TAF-7
TafName|Eindeutige Identifikation des TAF, **muss geliefert werden**| tariffName | TAF-2-ID
Description|Kurze Beschreibung des TAF, **optional** | |HT/NT Tarif
Meters|Liste der mit dem TAF verbundenen Zähler, **muss geliefert werden**| meterId
MeteringPointId|Zählpunktbezeichnung, **muss geliefert werden**| usagePointId | DE00000000000000000000000000000001
SupplierId|ID des Lieferanten, **muss geliefert werden** | invoicingPartyId | EMT-BDEW
ConsumerId|ObjectID des Letztverbrauchers, dem die die Daten zugeordnet werden (Cosem Logical Device ohne .sm), **muss geliefert werden** | customerId | userID-001
Begin|Startzeitpunkt des Vertrags, **muss geliefert werden**|||
End|Endzeitpunkt des Vertrags, **optional**|||

#### Transparenzfunktion

Die Zurdnung zwischen Liefranten-XML-Datei und den Vertragsdaten des SMGWs wird über folgende Felder vorgenommen:

Lieferanten-XML|ContractInfo|AR 2418-6-Datei vom Gateway
--- | --- | --- 
nicht relevant |TafId == TAF7|
UsagePoint.usagePointId|MeteringPointId|UsagePoint.usagePointId
UsagePoint.AnalysisProfile.tariffId|TafName|UsagePoint.tariffName

#### TAF-6

TAF-6 wird als eigenes ``ContractInfo`` zurückgeliefert, welches sich nur druch die TAF-ID vom zugehörigen Vertrag unterscheidet.


### 3. Laden der Daten zum vom Verbraucher ausgewählten Vertrag mittels ``LoadData``

Lädt die Ablesung für den in ``AdapterContext`` angegebenen Vertrag. 

- Ist keine ``BillingPeriod`` angegeben, werden nur originären Meßwertlisten und ggf. die 
  Logbuch-Einträge abgerufen. Dies kann z.B. bei TAF-7 der Fall sein.

- Ist die Abrechnungsperiode noch nicht abgeschlossen, werden nur die originären Meßwertlisten und ggf. die 
  Logbuch-Einträge abgerufen. Anschließend werden über ``GetCurrentRegisterValues`` die aktuellen 
  Registerwerte abgerufen.


### 4. Abruf der aktuellen Registerwerte durch ``GetCurrentRegisterValues``

Lädt die aktuellen Registerwerte (Wichtig: die abgeleiteten Register!) des angegebenen Vertrags. 

## Test-HAN-Adapter ``TRuDI.HanAdapter.Example``

Dieser Adapter dient zum Simulieren eines SMGWs und ist wärend der normalen Progammausführung nicht aktiv.

Um diesen zu aktivieren, muss das Programm mit dem Paramter ``--test=<Konfigurations-Datei>`` aufgerufen werden.

## TAF-Adapter

TAF-Adapter implementieren das Interface ```ITafAdapter``` aus dem Projekt ```TRuDI.TafAdapter.Interface```.

Ein TAF-Adapter liefert neben den berechneten Daten auch jeweils eine View-Komponente für die Zusammenfassung und Detail-Ansicht.

### Einbinden eines neuen TAF-Adapters

Die TAF-Adapter werden über das Projekt TRuDI.TafAdapter.Repository eingebunden. 

In der Klasse 

```csharp
TRuDI.TafAdapter.Repository.TafAdapterRepository
```

muss der neue TAF-Adapter in die Liste ```availableAdapters``` eingetragen werden.

Z.B.: 

```csharp
new TafAdapterInfo(TafId.Taf1, "Standard Adapter für TAF-1", typeof(TafAdapterTaf1)),
```

### Interface ``ITafAdapter``

Das Interface ``ITafAdapter`` enthält nur die Methode ``Calculate()``. Als Parameter werden die vom SMGW 
abgelesenen Daten sowie die XML-Datei des Lieferanten übergeben. Diese Methode liefert als Ergebnis ``TafAdapterData`` zurück. 

## TAF-Adapter-Test-Programm

Das Projekt TRuDI.TafAdapter.Test dient als Test-Programm für die TAF-Adapter. Damit können diese über die Kommandozeile aufgerufen werden.

```
Usage: TRuDI.TafAdapter.Test [options]

Options:
  -o|--output <output-file>  Ausgabedatei.
  -d|--data <data-file>      Daten aus dem SMGW.
  -t|--taf <taf-file>        TAF-Datei.
  -? | -h | --help           Show help information
```

### Beispiel

```
dotnet TRuDI.TafAdapter.Test.dll --data taf7-daten.xml --taf tariff-datei-vom-lieferanten.xml --output berechnete-tariff-register.txt
```

## Starten von TRuDI aus Visual Studio

Um TRuDI aus Visual Studio heraus zu starten, sind folgende Einstellungen auf der Seite Debug des Projekts TRuDI.Backend nötig:

![Debug-Einstellungen](doc/Images/Debug-Settings_VS2017.png)

- Launch: Project
- App URL: http://localhost:5000/

Wird TRuDI.Backend mit der Debug-Konfiguration kompiliert und gestartet, so wird immer der Port 5000 für die Webseite verwendet.

## Programm-Argumente von TRuDI

#### ``-l|--log <logfile>``

Datei in der Programmmeldungen protokolliert werden.

#### ``--loglevel <loglevel>``

Log Level: ``verbose``, ``debug``, ``info``, ``warning``, ``error``, ``fatal``. Standard ist ``info``.

#### ``-t|--test <testconfig>``

Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.



## Erzeugen eines Live-Images für Ubuntu

(wird aktuell noch erstellt)

