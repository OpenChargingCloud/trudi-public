namespace TRuDI.HanAdapter.XmlValidation.Models.BasicData
{
    /// <summary>
    /// Die Klasse LogEntry spezifiziert Logeinträge.
    ///
    /// Eine Instanz der Klasse LogEntry:
    /// 
    /// - kann auf eine Instanz der Klasse LogEvent verweisen
    /// </summary>
    public class LogEntry
    {

        /// <summary>
        /// LogEvent spezifiziert Logereignisse. 
        /// </summary>
        public LogEvent LogEvent 
        {
            get; set;
        }

        /// <summary>
        /// Das Datelement recordNumber ist der eineindeutige Bezeichner des Logeintrags. 
        /// Dieser wird mit Ablegen des Eintrags im Logbuch durch die Ge-räte-Firmware erzeugt. 
        /// Das Datenelement ist optional anzugeben.
        /// </summary>
        public uint? RecordNumber
        {
            get; set;
        }

    }
}
