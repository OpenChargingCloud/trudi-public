
# TRuDI-TAF-Adapter

TAF-Adapter implementieren das Interface ```ITafAdapter``` aus dem Projekt ```TRuDI.TafAdapter.Interface```.

Ein TAF-Adapter liefert neben den berechneten Daten auch jeweils eine View-Komponente für die Zusammenfassung und Detail-Ansicht.

## Einbinden eines neuen TAF-Adapters

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

## Interface ``ITafAdapter``

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
