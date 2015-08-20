/*!
 * \file    Et0.cs
 *
 * \brief   Implements the models for the calculation of reference evapotranspiration.
 *          Two calculation approaches are provided: Penman-Monteith Equation if nessecary climate data is avilable,
 *          Hargreaves Algorith can be used else.
 *  
 * \author  Hunstock
 * \date    15.08.2015
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using atbApi;
using atbApi.data;
using local;

namespace atbApi
{
    /*!
     * \brief   Encapsulates the result of a ET0 calculation.
     *
     */

    public class Et0Result
    {
        /*! , unit: "mm", description: Calculated ET0 value. */
        public double ra { get; set; }
        /*! , unit: "mm", description: Calculated ET0 value. */
        public double et0 { get; set; }

        public Et0Result(double ra, double et0)
        {
            this.ra = ra;
            this.et0 = et0;
        }
    }

    internal struct RaResult
    {
        public readonly double omegaS;
        public readonly double ra;
        public RaResult(double omegaS, double ra)
        {
            this.omegaS = omegaS;
            this.ra = ra;
        }
    }

    internal static class Ra
    {
        internal static RaResult RaCalc(
            DateTime date,
            Location location
        )
        {
            var yearLength = DateTime.IsLeapYear(date.Year) ? 366 : 365;
            var doy = date.DayOfYear;

            var dr = 1 + 0.033 * Math.Cos(2 * Math.PI / yearLength * doy);
            var delta = 0.409 * Math.Sin(2 * Math.PI / yearLength * doy - 1.39);
            var phi = Math.PI / 180 * location.lat;
            var s = -Math.Tan(phi) * Math.Tan(delta);
            s = Math.Min(1, s);
            s = Math.Max(-1, s);
            var omegaS = Math.Acos(s);
            var ra = 1440 / Math.PI * 0.082 * dr * (omegaS * Math.Sin(phi) * Math.Sin(delta) + Math.Cos(phi) * Math.Cos(delta) * Math.Sin(omegaS));

            return new RaResult(omegaS, ra);
        }
    }

    /*!
     * \brief   Model to calculate ET0 according to FAO paper 56. Two methods are available:
     *          Et0CalcPm():    Penman-Monteith equation
     *          Et0CalcHg():    Hargreaves method for reduced datasets
     *          
     * \remarks To choose method automatically depending on data available, use Et0Calc()
     *
     */

    public static class Et0
    {
        public static Et0Result Et0Calc(
            Climate climate,
            DateTime date,
            Location location
        )
        {
            return Et0Calc(climate, date, location, null, null, null, null, null);
        }
        /*!
         * \brief   Use this method to choose model for ET0 calculation automatically depending on available data.
         *
         * \param   climate     Parameter to relay to matching ET0 calculation model.
         * \param   date        Parameter to relay to matching ET0 calculation model.
         * \param   location    Parameter to relay to matching ET0 calculation model.
         * \param   _as         Relayed to Et0CalcPm()
         * \param   _bs         Relayed to Et0CalcPm()
         * \param   ct          Relayed to Et0CalcHg()
         * \param   ch          Relayed to Et0CalcHg()
         * \param   eh          Relayed to Et0CalcHg()
         *
         * \return  An Et0Result structure.
         */

        public static Et0Result Et0Calc(
            Climate climate,
            DateTime date,
            Location location,
            Double? _as,
            Double? _bs,
            Double? ct,
            Double? ch,
            Double? eh
        )
        {
            var climateSet = climate.getValues(date);

            //check availability of values
            if (climateSet == null) return null;
            if (
                   climateSet.max_temp != null
                && climateSet.min_temp != null
                && climateSet.humidity != null
                && (climateSet.Rs != null || climateSet.sunshine_duration != null)
            )
                //data sufficient for Penman-Monteith
                return Et0CalcPm(climate, date, location, _as, _bs);
            //use Hargreaves
            return Et0CalcHg(climate, date, location, ct, ch, eh);
        }


        public static Et0Result Et0CalcPm(
            Climate climate,
            DateTime date,
            Location location
        )
        {
            return Et0CalcPm(climate, date, location, null, null);
        }

        /*!
         * \brief   Calculate ET0 with Penman-Monteith method as described in FAO paper 56.
         *
         * \param   climate     Use this dataset with climate data.
         * \param   date        Date for ET0 calculation.
         * \param   location    The location for calculation, latitude and altitude are used.
         * \param   _as         Regression coefficient for calculation of global radiation. If omitted, FAO recommended default value of 0.25 is used.
         * \param   _bs         Regression coefficient for calculation of global radiation. If omitted, FAO recommended default value of 0.5 is used.
         *
         * \return  An Et0Result structure.
         */

        public static Et0Result Et0CalcPm(
            Climate climate,
            DateTime date,
            Location location,
            Double? _as,
            Double? _bs
        )
        {
            var climateSet = climate.getValues(date);

            //check availability of values
            if (climateSet == null) return null;
            if (
                   climateSet.max_temp == null
                || climateSet.min_temp == null
                || climateSet.humidity == null
                || (climateSet.Rs == null && climateSet.sunshine_duration == null)
            ) return null;

            //initialize default values and replace missing ones
            if (climateSet.mean_temp == null) climateSet.mean_temp = (climateSet.min_temp + climateSet.max_temp) / 2;
            //FAO recommended value default of 2m/s
            if (climateSet.windspeed == null) climateSet.windspeed = 2;
            if (_as == null) _as = 0.25;
            if (_bs == null) _bs = 0.5;
            if (location.alt == null) location.alt = 0;

            var raResult = Ra.RaCalc(date, location);
            var ra = raResult.ra;

            if (ra == 0)
            {
                return new Et0Result(0, 0);
            }

            var g = 0;
            var e0Tmin= 0.6108 * Math.Exp(17.27 * (double)climateSet.min_temp / ((double)climateSet.min_temp + 237.3));
            var e0Tmax= 0.6108 * Math.Exp(17.27 * (double)climateSet.max_temp / ((double)climateSet.max_temp + 237.3));
            var es= (e0Tmin + e0Tmax) / 2;
            var ea= climateSet.humidity / 100 * es;
            var n= 24 / Math.PI * raResult.omegaS;
            var rs0= (0.75 + 0.00002 * location.alt) * ra;
            var rs= climateSet.Rs;
            if (rs == null) {
                rs= (_as + _bs * climateSet.sunshine_duration  /  n) * ra;
                rs= Math.Min((double)rs, (double)rs0);
            }
            var rsDivRs0= Math.Min((double)rs / (double)rs0, 1);
            var rnl= 4.903e-9 * ((Math.Pow((double)climateSet.max_temp + 273.16, 4) + Math.Pow((double)climateSet.min_temp + 273.16, 4)) / 2) * (0.34 - 0.14 * Math.Sqrt((double)ea)) * (1.35 * rsDivRs0 - 0.35);
            var rns= 0.77 * rs;
            var rn= rns - rnl;
            var p= 101.3 * Math.Pow((293 - 0.0065 * (double)location.alt) / 293, 5.26);
            var gamma= 0.000665 * p;
            var d= 4098 * (0.6108 * Math.Exp(17.27 * (double)climateSet.mean_temp / ((double)climateSet.mean_temp + 237.3))) / Math.Pow((double)climateSet.mean_temp + 237.2, 2);
            var et0x= (0.408 * d * (rn - g) + gamma * (900 / ((double)climateSet.mean_temp + 273)) * (double)climateSet.windspeed * (es - ea)) / (d + gamma * (1 + 0.34 * (double)climateSet.windspeed));
            var et0 = Math.Max(0, (double)et0x);

            return new Et0Result(ra, et0);
        }


        public static Et0Result Et0CalcHg(
            Climate climate,
            DateTime date,
            Location location
        )
        {
            return Et0CalcHg(climate, date, location, null, null, null);
        }

        /*!
         * \brief   Calculate ET0 with Hargreaves method as described in FAO paper 56.
         *
         * \param   climate     Use this dataset with climate data.
         * \param   date        Date for ET0 calculation.
         * \param   location    The location for calculation, latitude and altitude are used.
         * \param   ct          May be null, default: 17.8, Empirical temperature coefficient, if omitted, recommended default value of 17.8 (Hargreaves 1994) is used.
         * \param   ch          May be null, default: 0.0023, Empirical Hargreaves coefficient, if omitted, recommended default value of 0.0023 (Hargreaves 1994) is used.
         * \param   eh          May be null, default: 0.5, Empirical hargreaves exponent, if omitted, recommended default value of 0.5 (Hargreaves 1994) is used.
         *
         * \return  An Et0Result structure.
         */

        public static Et0Result Et0CalcHg(
            Climate climate,
            DateTime date,
            Location location,
            Double? ct,
            Double? ch,
            Double? eh
        )
        {

            var climateSet= climate.getValues(date);
            if (climateSet == null) return null;
            if (
                   climateSet.max_temp == null
                || climateSet.min_temp == null
            ) return null;

            var raResult = Ra.RaCalc(date, location);
            var ra = raResult.ra;

            if (ra == 0) {
                return new Et0Result(0, 0);
            }

            if (ct == null) ct = 17.8;
            if (ch == null) ch = 0.0023;
            if (eh == null) eh = 0.5;
            var et0 = (double)ch * ra * Math.Pow(((double)climateSet.max_temp - (double)climateSet.min_temp), (double)eh) * (((double)climateSet.max_temp + (double)climateSet.min_temp) / 2 + (double)ct);

            return new Et0Result(ra, et0);
        }
    }
}
