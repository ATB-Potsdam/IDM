/*!
 * \file    GeoTools.cs
 *
 * \brief   Classes, structures and functions to work with geographic data.
 *
 * \author	Hunstock
 * \date     11.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace atbApi
{

    namespace tools
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

            /*!
             * \brief   Constructor without altitude.
             *
             * \param   lat The latitude.
             * \param   lon The longitude.
             */

            public Location(Double lat, Double lon)
            {
                this.lon = lon;
                this.lat = lat;
                this.alt = null;
            }

            /*!
             * \brief   Constructor.
             *
             * \param   lat The latitude.
             * \param   lon The longitude.
             * \param   alt The altitude.
             */

            public Location(Double lat, Double lon, Double alt)
            {
                this.lon = lon;
                this.lat = lat;
                this.alt = alt;
            }
        }
    }
}
