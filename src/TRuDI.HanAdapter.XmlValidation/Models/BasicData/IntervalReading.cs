﻿namespace TRuDI.HanAdapter.XmlValidation.Models.BasicData
{
    using System;

    /// <summary>
    /// Die Klasse IntervalReading repräsentiert die Daten zu einem konkreten Messwert. Jede 
    /// Nachricht muss mindestens eine Instanz der Klasse IntervalReading enthalten.
    /// </summary>
    public class IntervalReading
    {
        /// <summary>
        /// Verweis auf den IntervalBlock, der die Instanz der Klasse IntervalReading enthält.
        /// </summary>
        public IntervalBlock IntervalBlock 
        {
            get; set;
        }

        /// <summary>
        /// Das Datenelement timePeriod beschreibt das Intervall, für das der 
        /// angegebene Messwert gültig ist.
        /// 
        /// Das Intervall wird durch einen Startzeitpunkt und eine Dauer definiert.
        /// 
        /// Der Startzeitpunkt wird als Xs:dateTime beschrieben. Für eichrechtlich 
        /// relevante Daten muss hier zwingend die "capture_time" eingetragen werden,
        /// also der sekundengenaue Zeitpunkt der Messwerterfasung. Dieser wird zur Überprüfung
        /// der inneren Signatur benötigt. Die Dauer wird als ganzzahliger Sekundenwert beschrieben.
        /// 
        /// Bei einem Zählerstandsgang ist die Dauer 0s, da es sich um einen Zeitpunkt handelt.
        /// 
        /// JEde Instanz der Klasse IntervalReading muss ein Datenelement vom Typ timePeriod enthalten.
        /// </summary>
        public Interval TimePeriod
        {
            get; set;
        }

        /// <summary>
        /// Das Datenelement value repräsentiert den Wert der Messung. Dieser wird als ganzzahliger Wert definiert.
        /// 
        /// Jede Instanz der Klasse IntervalReading muss ein Datenelement value enthalten.
        /// </summary>
        public long? Value
        {
            get; set;
        }

        /// <summary>
        /// Das Datenelement statusFNN repräsentiert das Statuswort, welches sich  aus dem Statuswort des Zählers 
        /// und des Smart Meter Gateways zusammensetzt. Dieses Statuswort wird laut FNN Lastenheft Smart Meter Gateway
        /// [FNN SMGW] als octet-String übertragen und wird für die Überprüfung der inneren Signatur benötigt.
        /// 
        /// Datenformat: Hex-Darstellung gespeichert als String 256
        /// 
        /// Eine Instanz der Klasse IntervalReading muss ein Datenelement statusFNN oder statusPTB beinhalten. Wenn 
        /// eine Prüfung der Signatur durchgeführt werden soll, muss das Datenelement zwingend enthalten sein.
        /// </summary>
        public string StatusFNN
        {
            //TODO StatusFNN Enum erstellen (FLAGS)
            get; set;
        }

        //TODO StatusPTB  und StatusFNN Enums? Übernahme ins Pflichtenheft (SIEHE PTB 50.8)        
        /// <summary>
        /// Das Datenelement statusPTB repräsentiert das Statuswort des Zählers nach PTB 50.8. 
        /// Dieses Statuswort wird laut FNN Lastenheft Smart Meter Gateway [FNN SMGW] als
        /// octet-String übertragen. 
        /// 
        /// Eine Instanz der Klasse INtervalReading muss ein Datenelement statusFNN oder statusPTB beinhalten.
        /// </summary>
        public StatusPTB? StatusPTB
        {
            get; set;
        }

        /// <summary>
        /// Über die Funktion GetFormattedStatus erhält man das Statuswort, dass zur Anzeige 
        /// gebracht wird.
        /// </summary>
        /// <returns>Gibt das ausgewählte Statuswort zurück</returns>
        public string GetFormattedStatus()
        {
            //TODO Funktion zur Auswahl von statusFNN oder StatusPTB implementieren
            throw new NotImplementedException();
        }
    }
}
