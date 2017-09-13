namespace TRuDI.Models.BasicData
{
    /// <summary>
    /// kind beschreibt als Datenelement die konkrete Sparte des Zählpunktes.
    /// Gültige Werte nach ESPI REQ.21 sind:
    ///     0 – electricity(Elektrizität)
    ///     1 – gas(Gas)
    ///     2 – water(Wasser)
    ///     4 – pressure(Druck)
    ///     5 – heat(Wärme)
    ///     6 – cold(Kälte)
    ///     7 – communication(Kommunikation)
    ///     8 – time(Zeit)
    ///     
    /// Kind findet man in der Klasse ServiceCategory wieder.
    /// </summary>
    public enum Kind : ushort
    {
        electricity = 0,
        gas = 1,
        water = 2,
        pressure = 4,
        heat = 5,
        cold = 6,
        communication = 7,
        time = 8
    }
}
