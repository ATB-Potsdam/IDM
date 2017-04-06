/*!
 * \file    Evaporation.cs
 *
 * \brief   Implements the models for the calculation of evaporation as described in FAO paper 56.
 *         
 * \author  Hunstock
 * \date    15.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using atbApi;
using atbApi.data;
using local;

namespace atbApi
{
    /*!
     * \brief   Encapsulates the result of an evaporation calculation.
     *
     */

    public class EvaporationResult
    {
        /*! kcMin, unit: "none", description: crop coefficient minimal value */
        public double kcMin { get; set; }
        /*! kcMax, unit: "none", description: crop coefficient maximal value */
        public double kcMax { get; set; }
        /*! kcb, unit: "none", description: basal crop coefficient */
        public double kcb { get; set; }
        /*! fc, unit: "none", description: fraction of soil surface covered by vegetation */
        public double fc { get; set; }
        /*! few, unit: "none", description: fraction of soil that is exposed and wetted in the event */
        public double few { get; set; }
        /*! tew, unit: "mm", description: totally evaporable water */
        public double tew { get; set; }
        /*! rew, unit: "mm", description: readily evaporable water */
        public double rew { get; set; }
        /*! kr, unit: "none", description: evaporation reduction coefficient */
        public double kr { get; set; }
        /*! ke, unit: "mm", description: soil evaporation coefficient */
        public double ke { get; set; }
        /*! de, unit: "mm", description: drainage in evaporation layer */
        public double de { get; set; }
        /*! dpe, unit: "mm", description: percolation from evaporation layer to deeper soil layers */
        public double dpe { get; set; }
        /*! e, unit: "mm", description: Calculated evaporation value. */
        public double e { get; set; }
    }

    /*!
     * \brief   static class that holds all static functions for the evaporation calculation.
     *
     */

    public static class Evaporation
    {
        /*!
         * \brief   calculate evaporation.
         *
         * \param [in,out]  lastConditions  The last soil conditions.
         * \param   plantSet                Set the plant values.
         * \param   climateSet              Set the climate values.
         * \param   irrigationType          Type of the irrigation.
         * \param   et0                     The reference evapotranspiration.
         * \param   eFactor                 The factor e is reduced by. May be used to consider mulching etc.
         * \param   tew                     The totally evaporable water.
         * \param   irrigationFw            The fraction of wetted surface depending on irrigation type.
         * \param   autoIrrigationFw        The fraction of wetted surface depending on automatic irrigation type.
         * \param   netIrrigation           The netto irrigation.
         * \param   autoNetIrrigation       The automatic netto irrigation.
         * \param   interception            The interception.
         * \param   interceptionIrr         The interception of the irrigated water.
         * \param   interceptionAutoIrr     The interception of the automatic irrigated water.
         * \param [in,out]  eResult         The result of the calculation.
         *
         * \return  true if it succeeds, false if it fails.
         */

        public static bool ECalculation(
            ref SoilConditions lastConditions,
            PlantValues plantSet,
            ClimateValues climateSet,
            IrrigationType irrigationType,
            double et0,
            double eFactor,
            double tew,
            double irrigationFw,
            double autoIrrigationFw,
            double netIrrigation,
            double autoNetIrrigation,
            double interception,
            double interceptionIrr,
            double interceptionAutoIrr,
            ref EvaporationResult eResult
        )
        {
            if (eResult == null) eResult = new EvaporationResult();
            var rewFactor = 2.2926;
            var kcMax = 1.2;
            var kcMin = 0.0;
            var kcb = 0.0;
            var fc = 0.0;
            if (!(plantSet.isFallow == true))
            {
                kcb = (double)plantSet.Kcb;
                kcMax = 1.2 + (0.04 * ((double)climateSet.windspeed - 2) - 0.004 * ((double)climateSet.humidity - 45)) * Math.Pow(((double)plantSet.height / 3), 0.3);
                kcMax = Math.Max(kcMax, kcb + 0.05);
                kcMax = Math.Min(kcMax, 1.3);
                kcMax = Math.Max(kcMax, 1.05);
                kcMin = 0.175;
                fc = Math.Pow(Math.Max((kcb - kcMin), 0.01) / (kcMax - kcMin), 1 + 0.5 * (double)plantSet.height);
                fc = Math.Min(0.99, fc);
            }
            eResult.kcMin = kcMin;
            eResult.kcMax = kcMax;
            eResult.kcb = kcb;
            eResult.fc = fc;

            var few = Math.Min(1 - fc, Math.Min(irrigationFw, autoIrrigationFw));
            if ((netIrrigation > 0 || autoNetIrrigation > 0) && (irrigationType.name == "drip"))
            {
                few = Math.Min(1 - fc, (1 - (2 / 3) * fc) * Math.Min(irrigationFw, autoIrrigationFw));
            }

            eResult.few = few;
            eResult.tew = tew;
            var rew = tew / rewFactor;
            eResult.rew = rew;
            var kr = 1.0;
            if (lastConditions.de > rew)
            {
                kr = (tew - lastConditions.de) / (tew - rew);
                kr = Math.Max(0, kr);
            }
            if (rew == 0) kr = 0;
            eResult.kr = kr;
            var ke = Math.Min(kr * (kcMax - kcb), few * kcMax);
            eResult.ke = ke;
            var e = ke * et0;
            e = e * eFactor;
            eResult.e = e;
            var netPrecititationTc = (double)climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigation - interceptionAutoIrr;
            var dpe = netPrecititationTc - lastConditions.de;
            dpe = Math.Max(0, dpe);
            var de = lastConditions.de - netPrecititationTc + e / few + dpe;
            de = Math.Max(0, de);
            de = Math.Min(tew, de);
            lastConditions.de = de;
            lastConditions.dpe = dpe;
            eResult.de = de;
            eResult.dpe = dpe;
            return true;
        }
    }
}
