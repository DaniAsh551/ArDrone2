using System;
using Ghostware.NMEAParser.Enums;

namespace ArDrone2.Client.Location
{
    public class GpsState
    {
        /// <summary>
        /// 3D fix - values include:    1 = no fix
        //                              2 = 2D fix
        //                              3 = 3D fix
        /// </summary>
        public SatelliteFixType SatelliteFix { get; set; }

        /// <summary>
        /// PRNs of satellites used for fix(space for 12)
        /// </summary>
        public string Pnrs { get; set; }

        /// <summary>
        /// PDOP(dilution of precision)
        /// </summary>
        public float Pdop { get; set; }


        /// <summary>
        /// Horizontal dilution of precision(HDOP)
        /// </summary>
        public float Hdop { get; set; }

        /// <summary>
        /// Vertical dilution of precision(VDOP)
        /// </summary>
        public float Vdop { get; set; }
        
        public GpsFixQuality FixQuality { get; set; }

        public int NumberOfSatellites { get; set; }
    }
}