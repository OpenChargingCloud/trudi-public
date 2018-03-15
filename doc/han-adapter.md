# TRuDI-HAN-Adapter

## Erstellen eines neuen HAN-Adapters

Ein neues Projekt soll der Projektmappe hinzugefügt werden. Als Zielframework muss .NET Core 2.0 ausgewählt werden. Als Projekttyp soll Klassenbibliothek ausgewählt werden.

### Namenskonvention HAN-Adapter

Projektname soll nach folgendem Schema gebildet werden: ``TRuDI.HanAdapter.<Adaptername>``.

Dabei ist `"<Adaptername>"` durch den gewünschten Namen des Gateway-Herstellers zu ersetzten.
Es können z.B. die 3 Buchstaben der FLAG Hersteller ID verwendet werden, aber auch der vollständige Herstellername ist gültig.

Assemblies müssen folgendermaßen benannt werden (analog zum Projektnamen), z.B.:

```csharp
TRuDI.HanAdapter.<Adaptername>
```

Die Klasse, welche das Interface IHanAdapter impelmentiert, muss wie folgt benannt werden (ebenfalls wieder wie der Projektname):

```csharp
namespace TRuDI.HanAdapter.<Adaptername>
{
   public class HanAdapter<Adaptername> : IHanAdapter
   {
      // ...
   }
}
```

### Abhängigkeiten und NuGet Pakete

Eine Externe NuGet Paketquelle muss zum Projekt hinzugefügt werden. 
Als Quelle wird der Ordner ``private-packages`` aus dem Repository verwendet.

Das Paket ``IVU.Http`` aus dieser Quelle ist dem Projekt hinzuzufügen.

Es müssen auch Verweise auf Projekte ``TRuDI.HanAdapter.Interface`` und ``TRuDI.Models`` hinzugefügt werden.

Außerdem müssen auch die öffentliche NuGet Pakete 
- ``Microsoft.AspNetCore.All`` 
- ``Newtonsoft.Json`` 
 
dem Projekt hinzugefügt werden.

### Beispiel-Projekt-Datei

```XML
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

  <PropertyGroup>
    <!--  -->
    <RestoreSources>$(RestoreSources);../../private-packages;https://api.nuget.org/v3/index.json</RestoreSources>
    <PackageOutputPath>..\..\private-packages</PackageOutputPath>

    <!-- NuGet-Paket beim Build erstellen: true -->
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>

    <!-- Nach Änderungen am Adapter muss die Versions-Nummer entsprechend angepasst werden: -->
    <Version>1.0.0</Version>
    
    <!-- Diese Angaben sollten entsprechend angepasst werden: -->
    <Company>Hersteller-Name</Company>
    <Authors>Hersteller-Name</Authors>
    <Product>TRuDI-HAN-Adapter für Produktname des Herstellers</Product>
  </PropertyGroup>

  <!-- Für das Logging wird LibLog (siehe unten) verwendet: LIBLOG_PORTABLE muss hierzu definiert werden: -->
  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
    <DefineConstants>TRACE;DEBUG;NETCOREAPP2_0;LIBLOG_PORTABLE</DefineConstants>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
    <DefineConstants>TRACE;RELEASE;NETCOREAPP2_0;LIBLOG_PORTABLE</DefineConstants>
  </PropertyGroup>

  <!-- Hier ist <Adaptername> entsprechend zu ersetzten (und die Dateien natürlich auch anzulegen)! -->
  <ItemGroup>
    <EmbeddedResource Include="Content\bild_vom_smgw.jpg" />
    <EmbeddedResource Include="Views\Shared\Components\GatewayImage<Adaptername>View\Default.cshtml" />
  </ItemGroup>
  
  <!-- Hier werden die benötigten Nuget-Pakete eingebunden: -->
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.3" />
    <PackageReference Include="Newtonsoft.Json" Version="10.0.3" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\IVU.Http\IVU.Http.csproj" />
    <ProjectReference Include="..\TRuDI.HanAdapter.Interface\TRuDI.HanAdapter.Interface.csproj" />
    <ProjectReference Include="..\TRuDI.Models\TRuDI.Models.csproj" />
  </ItemGroup>

</Project>
```

### Ordnerstruktur

Hier kann das Projekt ``TRuDI.HanAdapter.Example`` als Beispiel genommen werden.

```
Projektverzeichnis (TRuDI.HanAdapter.<Adaptername>)
|
+- Components
|  
+- Content
|
+- Views
   |
   +- Shared
      |
      +- Components
         |
         +- GatewayImage<Adaptername>View
```

In den Ordner ``Content`` muss das Bild des Smart Meter Gateways als PNG-Datei in der Auflösung von mind. 320 x 320 Pixel als eingebettete Ressource angelegt werden.
Dem Ordner ``GatewayImage<Adaptername>View`` soll die View-Datei ``Default.cshtml`` mit foldendem Inhalt hinzugefügt werden:

```xml
<div>
    <img src="/resources/Content/<Name der Bild-Datei>.png" class="img-responsive"/>
</div>
```

Dem Ordner ``Components`` sollen zwei Klassendateien hinzugefügt werden: ``GatewayImage<Adaptername>View.cs`` und ``GatewayImage<Adaptername>ViewModel.cs``. Der Inhalt dieser Dateien kann 
aus entsprechenden Dateien im Projekt ``TRuDI.HanAdapter.Example`` entnommen werden.

### Projektstruktur

Die Projektstruktur sollte jetzt wie folgt aussehen (als Beispiel-Adaptername wurde *Mfc* gewählt):

![HAN-Adapter Projektstruktur](Images/HAN-Adapter_Struktur.png)

### IHanAdapter Schnittstelle

Die Zentrale Klasse des HAN.Adapters ``HanAdapter<AdapterName>`` muss die Schnittstelle ``IHanAdapter`` implementieren.
Dabei kann die Property ``SmgwImageViewComponent`` wie folgt implementiert werden:

```csharp
public Type SmgwImageViewComponent => typeof(GatewayImageMfcView);
```

Die Property ``ManufacturerParametersViewComponent`` kann auf ``null`` gesetzt werden.
Diese View-Komponente wird nur dann verwendet, wenn ein Smart Meter Gateway spezielle Parameter (z.B. bei dem Verbindungsaufbau) benötigt.


## Einbinden des neuen HAN-Adapters

Der HAN-Adapter soll wie folgt in die TRuDI Anwendung eingebunden werden.
In der Klasse (Projekt TRuDI.HanAdapter.Repository)

```csharp
TRuDI.HanAdapter.Repository.HanAdapterRepository
```

muss der neue HAN-Adapter in die Liste ```availableAdapters``` eingetragen werden.

Z.B.: 

```csharp
new HanAdapterInfo("MFC", "Beispiel GmbH", typeof(HanAdapterMfc)),
```

Während der Entwicklung des HAN-Adapters kann dieser als Projekt-Referenz in das Projekt TRuDI.HanAdapter.Repository.csproj aufgenommen werden:

```xml
  ...
  <ItemGroup>
    <ProjectReference Include="..\TRuDI.HanAdapter.Example\TRuDI.HanAdapter.Example.csproj" />
  </ItemGroup>
```

Die spätere Integration erfolgt als NuGet-Paket. Beispiel für die entsprechenden Projekt-Einstellungen:
![Projekt-Einstellungen HAN-Adapter](Images/HAN-Adapter_VS2017_Package_Settings.png)

Die NuGet-Pakete der HAN-Adapter werden im Verzeichnis private-packages abgelegt.


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

Die Ausgabe der Log-Meldungen übernimmt dadurch der Logger im aufrufenden Programm. 
In TRuDI wird hierzu Serilog (https://serilog.net/) verwendet.

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
    
    Bei TAF-7 sind es die originären Zählerstände vom Zähler. Ansonsten die aktuellen abgeleiteten Register.
   

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

Lädt die aktuellen Registerwerte des angegebenen Vertrags.

Wichtig: bei TAF-1 und TAF-2 die abgeleiteten Register. Bei TAF-7 die aktuellen originären Zählerstände. 

## Test-HAN-Adapter ``TRuDI.HanAdapter.Example``

Dieser Adapter dient zum Simulieren eines SMGWs und ist wärend der normalen Progammausführung nicht aktiv.

Um diesen zu aktivieren, muss das Programm mit dem Paramter ``--test=<Konfigurations-Datei>`` aufgerufen werden.
