using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace local
{

    /*!
     * \brief	Defines a data structure representing the GIS location. WGS84 datum (EPSG-code: 4326) for direct using within Google Earth
     * 
     */

    public struct Location
    {
        /*! logitude in decimal degrees */
        public double lon;
        /*! latitude in decimal degrees */
        public double lat;
        /*! altitude in meter above sea level, optional, may be null */
        public Double? alt;
    }

}
