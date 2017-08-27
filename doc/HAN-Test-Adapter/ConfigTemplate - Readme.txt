Kurze Erläuterungen zur Nutzung des json templates zur generierung neuer TRuDIHanAdpaterExample Daten
(Es gibt zu den Tarifstufen Taf1, Taf2 und Taf7 jeweils eine Beispiel json Konfiguration.)



{
  "DeviceId": "", (Die SmgwId. Muss zur Validierung mit der SmgwId in XmlConfig übereinstimmen.)
  "User": "", (Anmeldename am SMGW.)
  "Password": "", (Passwort für die Authentifizierung am SMGW.)
  "IPAddresss": "", (Die IPAdresse des SMGW.)
  "IPPort": "", (Der genutzte Port des SMGW.)
  "TimeToConnect": "", (Simuliert die Dauer des Verbindungsaufbaus.)
  "Cert": "", (Das Zertifikat, dass vom SMGW zurückgeliefert wird.)
  "Version": { (Die Firmware Version des SMGW. (Bsp: Component: "System", Version: "0.1", Hash: "a8c7d8938b3239c739cd"))
    "Component": "",
    "Version": "",
    "Hash": ""
  },
  "Contracts": [ (Die Verträge des SMGW.)
    {
      "TafId": "",
      "TafName": "",
      "Description": "",
      "Meters": [
        ""
      ],
      "SupplierId": "",
      "ConsumerId": "",
      "Begin": "",
      "End": "",
      "BillingPeriods": [ (Die Billing Period wird abgerufen und anhand dieser Periode werden Daten zurückgeliefert.)
        {
          "Begin": "", 
          "End": ""
        }
      ]
    }
  ],
  "WithLogData": "", (Dieser Wert kann später überschrieben werden.Gibt an ob LogDaten mit ausgelesen werden oder nicht.)
  "XmlConfig": { (Die Konfiguration zum Erzeugen der Xml Datei.
    "UsagePointId": "",
    "TariffName": "",
    "TariffId": "",
    "CustomerId": "",
    "InvoicingPartyId": "",
    "SgmwId": "", (Siehe DeviceId.)
    "ServiceCategoryKind": "",
    "RandomLogCount": "", (Gibt an, ob eine zufällige Anzahl von LogEinträgen generiert werden soll. Ist er auf true wird LogCount ignoriert.)
    "LogCount": "", (Die Anzahl der LogEinträge. Siehe RandomLogCount)
    "PossibleLogMessages": [ (Die Meldungen, die in einem LogEvent ausgegeben werden können.)
      "",
      ""
    ],
    "Certificates": [
      {
        "Certificate": {
          "CertId": "",
          "CertType": "",
          "ParentCertId": ""
        },
        "CertContent": ""
      }
    ],
    "TariffUseCase":  "", (Wird für die Liferanten Xml bei Taf7 benötigt. z.B. Wert 2 für Taf2)
    "TariffStageCount": "", (Die Anzahl der Tarifstufen. Bei Taf2 wird es für die MeterReadings benötigt, bei Taf1 für das abgeleitete Register und bei Taf7 für die LieferantenXml)
    "DefaultTariffNumber": "", (Nötig für die LieferantenXml Taf7: Beschreibt den Standardtarif)
    "DayIdCount": "", (Nötig für die LieferantenXml Taf7: Gibt an wieviel verschiedene Tagesprofile es gibt)
    "ValueSummary": "", (Der gesamte simulierte Energieverbrauch während der Billing Period)
    "MeterReadingConfigs": [
      {
        "MeterId": "",
        "MeterReadingId": "",
        "IsOML": "", (Legt fest, ob das MeterReading eine OML darstellt.)
        "OMLInitValue": "", (Legt den initalen Wert einer OML fest.)
        "PeriodSeconds": "", (Die Abrechnungsperiode)
        "PowerOfTenMultiplier": "",
        "Uom": "",
        "Scaler": "",
        "ObisCode": "",
        "IntervalBlocks": [
          {
            "Duration": "",
            "Start": "",
            "UsedStatus": "" (Welcher Status benutzt werden soll. Hier gehen nur die Werte "FNN" oder "PTB")
          }
        ]
      }
    ],
    "TariffStageConfigs": [ (Wird für die Lieferanten Xml in Taf7 benötigt. Pro Tarifstufe muss ein TariffStageConfig angelegt werden.)
      {
        "TariffNumber": "",
        "Description": "",
        "ObisCode": ""
      }
    ],
    "DayProfiles": [ (Werden für die Lieferanten Xml benötigt. )
      {
        "DayId": "",
        "DayTimeProfiles": [
          {
            "Start": "",
            "End": "",
            "TariffNumber":  "" (Die zu dem Zeitraum gültige TariffNumber. Muss mit einer TariffNumber in einem TariffStageConfig übereinstimmen)
          }
        ]
      }
    ]
  }
}