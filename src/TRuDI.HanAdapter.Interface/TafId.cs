namespace TRuDI.HanAdapter.Interface
{
    /// <summary>
    /// Enumeration of TAF-IDs.
    /// </summary>
    public enum TafId
    {
        /// <summary>
        /// Datensparsamer Tarif
        /// </summary>
        Taf1 = 1,

        /// <summary>
        /// Zeitvariabler Tarif
        /// </summary>
        Taf2 = 2,

        /// <summary>
        /// Lastvariable Tarife
        /// </summary>
        Taf3 = 3,

        /// <summary>
        /// Verbrauchsvariable Tarife
        /// </summary>
        Taf4 = 4,

        /// <summary>
        /// Ereignisvariable Tarife
        /// </summary>
        Taf5 = 5,

        /// <summary>
        /// Abruf von Messwerten im Bedarfsfall
        /// </summary>
        Taf6 = 6,

        /// <summary>
        /// Zählerstandsgangsmessung
        /// </summary>
        Taf7 = 7,

        /// <summary>
        /// Erfassung von Extremwerten.
        /// </summary>
        Taf8 = 8,

        /// <summary>
        /// Abruf der Ist-Einspeisung einer Erzeugungsanlage.
        /// </summary>
        Taf9 = 9,
    }
}