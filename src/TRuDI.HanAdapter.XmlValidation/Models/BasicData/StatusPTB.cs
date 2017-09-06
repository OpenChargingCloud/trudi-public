namespace TRuDI.HanAdapter.XmlValidation.Models.BasicData
{
    /// <summary>
    /// Die Enumeration stellt die Kodierung von eichrechtlich releveanten Fehlern 
    /// laut PTB 50.8 dar
    /// 
    /// StatusPTB wird in IntervalReading benötigt.
    /// </summary>
    public enum StatusPTB : byte
    {
        //Kein Fehler
        No_Error = 0,

        //Warnung, keine (eichrechtliche) Aktion notwendig, Messwert gültig.
        Warning = 1,

        //Temporärer Fehler, gesendeter Messwert wird als ungültig gekennzeichnet, 
        //der Wert im Messwertfeld kann entsprechend den Regeln [VDE4400] bzw. [G685] im Backend als Ersatzwert verwendet werden.
        Temp_Error_signed_invalid = 2,

        //Temporärer Fehler, gesendeter Messwert ist ungültig, der im Messwertfeld enthaltene Wert kann im Backend nicht als Ersatzwert verwendet werden.
        Temp_Error_is_invalid = 3,

        //Fataler Fehler (Zähler defekt), der aktuell gesendete und alle zukünftigen Messwerte sind ungültig.
        Fatal_Error = 4
    }
}
