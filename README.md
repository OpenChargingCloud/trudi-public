# TRuDI - Transparenz- und Display-Software

Ansprechpartner:

	Thomas Müller
	IVU Softwareentwicklung GmbH
	tmueller@ivugmbh.de
	Tel.: 09471 / 30 73 237  

## Entwicklungsumgebung

### Microsoft .Net Core

- Visual Studio 2017 15.4: https://www.visualstudio.com/de/vs/
- .Net Core SDK 2.0: https://www.microsoft.com/net/core/
- Alternativ zu Visual Studio kann auch Visual Studio Code verwendet werden: https://code.visualstudio.com/

### Electron

- Node.js 6.11: https://nodejs.org/
- npm-Pakete:
  npm install electron-builder -g


## Installations-Pakete

System           | Download
---              | ---
Windows (64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-Setup-1.0.14-x86_64.exe
Windows (32-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-Setup-1.0.14-x86_32.exe
Linux ([AppImage](https://de.wikipedia.org/wiki/AppImage), 64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-1.0.14-x86_64.AppImage
Linux (deb, 64-Bit) | http://www.ivu-software.de/apps/TRuDI/TRuDI-1.0.14_amd64.deb

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