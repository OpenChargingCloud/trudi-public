﻿namespace TRuDI.HanAdapter.XmlValidation.Models.BasicData
{
    /// <summary>
    /// Die Enumeration CertType beschreibt den Zertifikattyp, der von 
    /// jeder Instanz der Klasse Certificate angegeben werden muss. 
    /// Dadurch wird jedes Zertifikat eindeutig klassifiziert. 
    /// </summary>
    public enum CertType : byte
    {
        Signatur = 1,
        SubCA,
        SBGW_HAN
    }
}