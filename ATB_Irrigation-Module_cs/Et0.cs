/*!
 * \file    Et0.cs
 *
 * \brief   Implements the models for the calculation of reference evapotranspiration.
 *          Two calculation approaches are provided: Penman-Monteith Equation if nessecary climate data is avilable,
 *          Hargreaves Algorith can be used else.
 *
 * 
 * \code{.unparsed}
            //example code to perform an et0 calculation for a single day
            //create climate class instance
            Climate climate = new Climate("test", TimeStep.day);
            //set date for calculation
            DateTime testDate = new DateTime(2015, 6, 12);
            //create values for that date
            ClimateValues testValues = new ClimateValues()
            {
                min_temp = 3.5,
                max_temp = 21.7,
                mean_temp = 11,
                precipitation = 0,
                windspeed = 2.6,
                humidity = 45,
                sunshine_duration = 4.7
            };
            //add values to climate
            climate.addValues(testDate, testValues);
            //create a location
            Location testLocation = new Location(42, -94, 330);

            //create result
            Et0Result testResult = new Et0Result();
            //do calculation - is result valid?
            if (Et0.Et0Calc(climate, testDate, testLocation, ref testResult))
            {
                return testResult.et0;
            }
            return null;
 * \endcode
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
     * \brief   Regression coefficients for ET0 calculation with Penman/Monteith.
     *          have to provided for any calculation
     *          can be reused for following calculations
     *
     */

    public class Et0PmArgs
    {
        private double __as;
        private double __bs;

        /*! read only property to access _as value */
        public double _as { get { return __as; } }
        /*! read only property to access _bs value */
        public double _bs { get { return __bs; } }

        /*!
         * \brief   Default constructor using default regression coefficients
         *          as: default: 0.25, (FAO56 paper)
         *          bs: default: 0.5, (FAO56 paper)
         *
         */

        public Et0PmArgs()
        {
            __as = 0.25;
            __bs = 0.5;
        }

        /*!
         * \brief   Custom constructor with self defined regression coefficients.
         *
         * \param   _as The "as" coefficient.
         * \param   _bs The "bs" coefficient.
         */

        public Et0PmArgs(double _as, double _bs)
        {
            this.__as = _as;
            this.__bs = _bs;
        }
    }

    /*!
     * \brief   Regression coefficients for ET0 calculation with Hargreaves method.
     *          have to provided for any calculation
     *          can be reused for following calculations
     *
     */

    public class Et0HgArgs
    {
        private double __ct;
        private double __ch;
        private double __eh;

        /*! read only property to access _ct value */
        public double _ct { get { return __ct; } }
        /*! read only property to access _ch value */
        public double _ch { get { return __ch; } }
        /*! read only property to access _eh value */
        public double _eh { get { return __eh; } }

        /*!
         * \brief   Default constructor using default regression coefficients
         *          ct: default: 17.8, Empirical temperature coefficient (Hargreaves 1994)
         *          ch: default: 0.0023, Empirical Hargreaves coefficient (Hargreaves 1994)
         *          eh: default: 0.5, Empirical hargreaves exponent (Hargreaves 1994)
         *
         */

        public Et0HgArgs()
        {
            __ct = 17.8;
            __ch = 0.0023;
            __eh = 0.5;
        }

        /*!
         * \brief   Custom constructor with self defined regression coefficients.
         *
         * \param   _ct     Empirical temperature coefficient, recommended default value of 17.8 (Hargreaves 1994)
         * \param   _ch     Empirical Hargreaves coefficient, recommended default value of 0.0023 (Hargreaves 1994)
         * \param   _eh     Empirical hargreaves exponent, recommended default value of 0.5 (Hargreaves 1994)
         */

        public Et0HgArgs(double _ct, double _ch, double _eh)
        {
            this.__ct = _ct;
            this.__ch = _ch;
            this.__eh = _eh;
        }
    }


    /*!
     * \brief   Encapsulates the result of an ET0 calculation.
     *
     */

    public class Et0Result
    {
        /*! extraterrestrial radiation, unit: "MJ m-² day-¹", description: Calculated extraterrestrial radiation. */
        public Double? ra { get; set; }
        /*! reference evapotranspiration, unit: "mm", description: Calculated ET0 value. */
        public Double? et0 { get; set; }

        /*!
         * \brief   Empty constructor, initializes result without values.
         *
         */

        public Et0Result()
        {
        }

        /*!
         * \brief   Constructor creates result with values.
         *
         * \param   ra  The ra value.
         * \param   et0 The et0 value.
         */

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
        private static Et0PmArgs _et0PmArgsDefault = new Et0PmArgs();
        private static Et0HgArgs _et0HgArgsDefault = new Et0HgArgs();

        public static bool Et0Calc(
            Climate climate,
            DateTime date,
            Location location,
            ref Et0Result result
        )
        {
            return Et0Calc(climate, date, location, _et0PmArgsDefault, _et0HgArgsDefault, ref result);
        }

        /*!
         * \brief   Use this method to choose model for ET0 calculation automatically depending on available data.
         *
         * \param   climate     Parameter to relay to matching ET0 calculation model.
         * \param   date        Parameter to relay to matching ET0 calculation model.
         * \param   location    Parameter to relay to matching ET0 calculation model.
         * \param   et0PmArgs   Relayed to Et0CalcPm()
         * \param   et0HgArgs   Relayed to Et0CalcHg()
         *
         * \return  An Et0Result structure.
         */

        public static bool Et0Calc(
            Climate climate,
            DateTime date,
            Location location,
            Et0PmArgs et0PmArgs,
            Et0HgArgs et0HgArgs,
            ref Et0Result result
        )
        {
            var climateSet = climate.getValues(date);

            //check availability of values
            if (climateSet == null) return false;
            if (
                   climateSet.max_temp.HasValue
                && climateSet.min_temp.HasValue
                && climateSet.humidity.HasValue
                && (climateSet.Rs.HasValue || climateSet.sunshine_duration.HasValue)
            )
                //data sufficient for Penman-Monteith
                return Et0CalcPm(climate, date, location, et0PmArgs, ref result);
            //use Hargreaves
            return Et0CalcHg(climate, date, location, et0HgArgs, ref result);
        }



        /*!
         * \brief   Calculate ET0 with Penman-Monteith method as described in FAO paper 56.
         *
         * \param   climate     Use this dataset with climate data.
         * \param   date        Date for ET0 calculation.
         * \param   location    The location for calculation, latitude and altitude are used.
         * \param   et0Args     Regression coefficients for Penman/Monteith calculation
         * \param   [in,out]    result  Result of calculation
         *
         * \return  true on success
         */

        public static bool Et0CalcPm(
            Climate climate,
            DateTime date,
            Location location,
            Et0PmArgs et0Args,
            ref Et0Result result
        )
        {
            var climateSet = climate.getValues(date);

            //check availability of values
            if (climateSet == null) return false;
            if (
                   !climateSet.max_temp.HasValue
                || !climateSet.min_temp.HasValue
                || !climateSet.humidity.HasValue
                || (!climateSet.Rs.HasValue && !climateSet.sunshine_duration.HasValue)
            ) return false;

            //initialize default values and replace missing ones
            if (!climateSet.mean_temp.HasValue) climateSet.mean_temp = (climateSet.min_temp + climateSet.max_temp) / 2;
            //FAO recommended value default of 2m/s
            if (!climateSet.windspeed.HasValue) climateSet.windspeed = 2;
            //copy altitude from climate, location cannot be null
            if (!location.alt.HasValue)
            {
                if (climate.location.alt.HasValue)
                {
                    location.alt = climate.location.alt;
                }
                else {
                    location.alt = 0;
                }
            }

            var raResult = Ra.RaCalc(date, location);
            result.ra = raResult.ra;

            if (result.ra == 0)
            {
                result.et0 = 0;
                return true;
            }

            var g = 0;
            var e0Tmin= 0.6108 * Math.Exp(17.27 * (double)climateSet.min_temp / ((double)climateSet.min_temp + 237.3));
            var e0Tmax= 0.6108 * Math.Exp(17.27 * (double)climateSet.max_temp / ((double)climateSet.max_temp + 237.3));
            var es= (e0Tmin + e0Tmax) / 2;
            var ea= climateSet.humidity / 100 * es;
            var n= 24 / Math.PI * raResult.omegaS;
            var rs0 = (0.75 + 0.00002 * location.alt) * result.ra;
            var rs= climateSet.Rs;
            if (rs == null) {
                rs = (et0Args._as + et0Args._bs * climateSet.sunshine_duration / n) * result.ra;
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
            result.et0 = Math.Max(0, (double)et0x);

            return true;
        }


        /*!
         * \brief   Calculate ET0 with Hargreaves method as described in FAO paper 56.
         *
         * \param   climate     Use this dataset with climate data.
         * \param   date        Date for ET0 calculation.
         * \param   location    The location for calculation, latitude and altitude are used.
         * \param   et0Args     Regression coefficients for Hargreaves calculation
         * \param   [in,out]    result  Result of calculation
         *
         * \return  true on success
         */

        public static bool Et0CalcHg(
            Climate climate,
            DateTime date,
            Location location,
            Et0HgArgs et0Args,
            ref Et0Result result
        )
        {

            var climateSet= climate.getValues(date);
            if (climateSet == null) return false;
            if (
                   !climateSet.max_temp.HasValue
                || !climateSet.min_temp.HasValue
            ) return false;

            var raResult = Ra.RaCalc(date, location);
            result.ra = raResult.ra;

            if (result.ra == 0) {
                result.et0 = 0;
                return true;
            }

            var lambda = 2.5 - 0.002361 * ((double)climateSet.max_temp + (double)climateSet.min_temp) / 2;
            result.et0 = et0Args._ch * result.ra * Math.Pow(((double)climateSet.max_temp - (double)climateSet.min_temp), et0Args._eh) * (((double)climateSet.max_temp + (double)climateSet.min_temp) / 2 + et0Args._ct) / (double)lambda;

            return true;
        }
    }
}
