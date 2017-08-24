--------------------------------------------------------------------------------------------------
Nutzung des configTemplate.json um Daten für den TRuDIHanAdapter "Example" zu erzeugen.
--------------------------------------------------------------------------------------------------
Die nicht benötigten Datenfelder müssen gelöscht werden. Ansonsten ist die Json Datei fehlerhaft.
--------------------------------------------------------------------------------------------------

{
	"DeviceId":"", 	------------------------------------------------> Hier wird die SmgwId eingetragen, die auch weiter unten im XmlConfig Element nochmals auftaucht.
	"User":"",		------------------------------------------------> Ein Benutzer, der dazu dient das LogIn mit Name und Passwort zu simulieren.
	"Password":"",  ------------------------------------------------> Ein Password, das dazu dient das LogIn mit Name und Passwort zu simulieren.
	"IPAddress":"",  -----------------------------------------------> Dient ebenfalls der Simulierung der Verbindung mit einem SMGW. Zusammen mit IPPort.
	"IPPort":"",
	"TimeToConnect":"", --------------------------------------------> Definiert eine Zeitspanne, die für die Verbindung benötigt wird. (Bsp: (00:00:30) für 30 Sekunden)
	"Cert":"",			--------------------------------------------> Ein Zertifikat um die Verbindung mit Zertifikat zu simulieren. (Als Hex-String anzugeben)
	"Version":			--------------------------------------------> Definition einer Firmwareversion mit den Datenelementen Component, Version und Hash. 
		{
			"Component":"",	
			"Version":"",
			"Hash":""
		},
	"Contracts":  --------------------------------------------------> Hier können ein oder mehrere Verträge festgelegt werden. 
	[
		{
			"TafId":"", --------------------------------------------> Die TafId des Tarifanwendungsfalls des jeweiligen Vertrages (z.B. 2 für Taf2).
			"TafName":"",  -----------------------------------------> Der Name des Tarifs. Muss sich mit dem Datenelement tariffName in der Xml Datei decken. 
			"Description":"", --------------------------------------> Eine Beschreibung des Tarifs.
			"Meters":         --------------------------------------> Hier werden die Meter (MeterId) aufgezählt, die mit dem Vertrag zusammenhängen. Sie tauchen auch weiter unten nochmal auf (MeterId in MeterReadingConfigs).
			[
				""
			],
			"SupplierId":"", ---------------------------------------> Die Id des Stromlieferanten. Tauch in der XmlConfig und InvoicingPartyId nochmals auf.
			"ConsumerId":"", ---------------------------------------> Identisch mit dem Feld CustomerId in XmlConfig. 
			"Begin":"",		 ---------------------------------------> Vertragsbeginn
			"End":"", ----------------------------------------------> Vertragsende
			"BillingPeriods": --------------------------------------> Hier besteht die Möglichkeit verschiedene Abrechnungszeiträume für die jeweiligen Verträge anzugeben.
			[														  Die erste BillingPeriod wird als Abrechnugszeitraum benutzt 	
				{
					"Begin":"",
					"End":""
				}
			]
		}
	],
	"WithLogData":"", ----------------------------------------------> Hier wird spezifiziert, ob die LogDaten mit ausgelesen werden sollen. Das Element AdapterContext ctx.WithLogdata überschreibt die Angabe.
	"XmlConfig": ---------------------------------------------------> In XmlConfig können Einstellungen für die simulierten SMGW Daten vorgenommen werden.
		{
			"UsagePointId":"", 
			"TariffName":"",
			"CustomerId":"",
			"InvoicingPartyId":"",
			"SgmwId":"",
			"ServiceCategoryKind":"", 
			"RandomLogCount":"", -----------------------------------> Steht RandomLogCount auf true, wird eine zufällige Anzahl an LogEinträgen generiert.
			"LogCount":"", -----------------------------------------> Hier kann die gewünschte Anzahl an LogEinträgen festgelegt werden. HINWEIS: Steht RandomLogCount auf true, wird die Zahl ignoriert.
			"PossibleLogMessages": ---------------------------------> Hier können Nachrichten eingegeben werden, die bei den LogEinträgen als Meldung erscheinen. Sie werden per Zufall ausgewählt.
			[
				"",
				""
			],
			"Certificates": ----------------------------------------> Hier können die relevanten Zertifikate eingetragen werden. 
			[
				{
					"Certificate":
						{
							"CertId":"",			
							"CertType":"",
							"ParentCertId":""
						},
					"CertContent":"" --------------------------------> Der Inhalt des Zertifikates (Als HexString anzugeben)
				}
			],
			"TariffStageCount":"", ----------------------------------> Angabe der Tarifstufen bei Taf2 (Beispiel: "2" Damit müssen 5 MeterReadingConfigs generiert werden, da die OML, Das Summenregister und 
																	   das Fehlerregister zu den Tarifstufen hinzugefügt werden muss.		
			"TariffId":"",         ----------------------------------> Wird benutzt um den QualifiedLogicalName zu bilden.
			"Taf2SummaryValue":"", ----------------------------------> Die Summe der Werte für den Tarifanwendungsfall 2.
			"OMLValue":"",		   ----------------------------------> Der Wert, der in der OML stehen soll.
			"MeterReadingConfigs": ----------------------------------> Hier werden die MeterReading Elemente konfiguriert. 
			[
				{
					"MeterId":"",
					"MeterReading":"",
					"IsOML":"",  ------------------------------------> Ist IsOML auf true, wird das MeterReading Element als Ordinäre Messwertliste behandelt.
					"Taf7InitValue":"", -----------------------------> Der Initialwert für Taf7.
					"Taf7MaxStepValue":"", --------------------------> Der maximale Wert, der pro Messpunkt hinzugefügt werden darf. 
					"Taf7periodMinutes":"", -------------------------> Die Länge der Abbrechnungsperiode (mindestens 15 und nur ein vielfaches von 15)
					"PowerOfTenMultiplier":"",
					"Uom":"",
					"Scaler":"",
					"ObisCode":"",
					"IntervalBlocks":
					[
						{
							"Duration":"",
							"Start":"",
							"UsedStatus":"", -----------------------> Hier kann der gewünschte Status angegeben werden. Es sind nur die beiden Werte "FNN" oder "PTB" gültig.
							"IntervalReadingCount":"" --------------> Die Anzahl der IntervalReadings. Wird bei Taf7 nicht benötigt.
						}
					]
				}
			]

		}
}