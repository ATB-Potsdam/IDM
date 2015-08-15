/*!
 * \file    KcIni.cs
 *
 * \brief   Implements the model for the calculation of kc for the initial plant stage.
 *  
 * \author  Hunstock
 * \date    15.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using atbApi;
using atbApi.data;
using local;

namespace atbApi
{
    /*!
     * \brief   Encapsulates the result of a kcIni calculation.
     *
     */

    public class KcIniResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public Double? runtimeMs { get; set; }
        /*! , unit: "none", description: Start date of initial plant stage. */
        public DateTime startDate { get; set; }
        /*! , unit: "none", description: End date of initial plant stage. */
        public DateTime endDate { get; set; }
        /*! , unit: "d", description: Length of initial plant stage. */
        public int initialLength { get; set; }
        /*! , unit: "none", description: Number of separate wetting events in initial plant stage. Consecutive days with wetting events are counted once. */
        public int eventCount { get; set; }
        /*! , unit: "none", description: Fraction of wetted surface sum for all days (precipitation and irrigation). */
        public Double? fwSum { get; set; }
        /*! , unit: "none", description: Number of days counted while building fwSum, used to calculate fw mean later. */
        public int fwCount { get; set; }
        /*! , unit: "mm", description: Sum of ET0 for initial plant stage. */
        public Double? et0Sum { get; set; }
        /*! , unit: "mm", description: Sum of evaporation for initial plant stage. */
        public Double? eSum { get; set; }
        /*! , unit: "mm", description: Sum of precipitation for initial plant stage. */
        public Double? pSum { get; set; }
        /*! , unit: "mm", description: Sum of irrigation for initial plant stage. */
        public Double? irrigation { get; set; }
        /*! , unit: "mm", description: Sum of predicted automatic irrigation for initial plant stage. */
        public Double? autoIrrigation { get; set; }
        /*! , unit: "mm", description: Mean time between wetting events. */
        public Double? tw { get; set; }
        /*! , unit: "none", description: Fraction of wetted surface mean value for all days with wetting. */
        public Double? fw { get; set; }
        /*! , unit: "mm", description: Mean value of ET0 for initial plant stage. */
        public Double? et0Mean { get; set; }
        /*! , unit: "mm", description: Mean value of precipitation plus irrigation per wetting event. Sonsecutive days of precipitation and/or irrigation are counted as one event. */
        public Double? pMean { get; set; }
        /*! , unit: "m", description: Depth of soil evaporation layer if this value is unknown FAO recommends a value between 0.1 and 0.15m. In this model a default value of 0.1 is used. */
        public Double? ze { get; set; }
        /*! , unit: "mm", description: Total evaporable water base value, if evaporation layer is saturated. */
        public Double? tewBase { get; set; }
        /*! , unit: "mm", description: Start condition for water deficit in evaporation layer. */
        public Double? depletionDeInitial { get; set; }
        /*! , unit: "none", description: First calculated kcIni value for 100% wetted surface and without limitation to 0.1 .. 1.15 */
        public Double? kcIni100MinMax { get; set; }
        /*! , unit: "mm", description: Bare soil evapotranspiration, ET0 multiplied by 1.15 as recommended by the FAO. */
        public Double? es0 { get; set; }
        /*! , unit: "mm", description: Total evaporable water, scaled tewBase value, depending on et0Mean. */
        public Double? tew { get; set; }
        /*! , unit: "mm", description: Readily evaporable water, scaled tew value, depending on soil structure. Here a fixed mean value of 2.2926 is used. */
        public Double? rew { get; set; }
        /*! , unit: "mm", description: Limited tew value, depending on available water. */
        public Double? tewCorr { get; set; }
        /*! , unit: "mm", description: Limited rew value, depending on available water. */
        public Double? rewCorr { get; set; }
        /*! , unit: "d", description: Mean time for drying phase 1. */
        public Double? t1 { get; set; }
        /*! , unit: "mm", description: Calculated kcIni value for 100% wetted surface. */
        public Double? kcIni100 { get; set; }
        /*! , unit: "mm", description: Calculated kcIni value, scaled by fw, fraction of wetted surface. */
        public Double? kcIni { get; set; }
    }

    /*!
     * \brief   Model to calculate kc for the initial plant stage according to FAO paper 56.
     *
     */

    internal class KcIni
    {
        /*!
         * \brief   function to run calculation.
         *
         * \param   climate             The climate dataset.
         * \param   soil                The soil data.
         * \param   irrigation          The irrigation schedule.
         * \param   et0                 The et0 values for calculation timespan.
         * \param   startDate           The start of the initial plant stage.
         * \param   initialLength       Length of the initial plant stage.
         * \param   location            The geographical location.
         * \param   delpetionDeInitial  The initial condition for depletion in evaporation layer.
         * \param   ze                  The depth of the evaporation layer.
         * \param   autoIrr             Set to true to automatically irrigate.
         * \param   autoIrrLevel        If "autoIrr" is true, it will start at this fraction of available soil water. The value 0.8 is for 80%.
         * \param   autoIrrCutoff       If "autoIrr" is true _and_ "autoIrrAmount" is 0 (automatic amount calculation), then the amount is calculated to saturate the soil right up to "autoIrrCutoff" value.
         * \param   autoIrrAmount       If "autoIrr" is true, this amount of irrigation is added per day, if available soil water drops below "autoIrrLevel". If this value is 0, then the amount of drainage from last day is added if available soil water drops below "autoIrrLevel".
         * \param   autoIrrType         If "autoIrr" is true, this type of irrigation system is used for calculation of interception and fraction wetted.
         * \param   eFactor             The calculated value of evaporation is always multiplied by this factor to reduce evaporation because of e.g. mulching.
         *
         * \return  A KcIniResult structure with kcIni value and intermediate results.
         */

        internal KcIniResult kcIniCalc(
            Climate climate,
            Soil soil,
            IrrigationSchedule irrigation,
            Dictionary<DateTime, Double> et0,
            DateTime startDate,
            int initialLength,
            Location location,
            double delpetionDeInitial,
            double ze,
            bool autoIrr,
            double autoIrrLevel,
            double autoIrrCutoff,
            double autoIrrAmount,
            IrrigationType autoIrrType,
            double eFactor
            )
        {
            KcIniResult result = new KcIniResult();
            result.kcIni = 0.8;
            return result;
        }
    }
}
