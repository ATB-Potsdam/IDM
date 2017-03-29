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
     * \brief   Encapsulates the result of an E calculation.
     *
     */

    public class EvaporationResult
    {
        /*! , unit: "none", description: crop coefficient minimal value */
        public double kcMin { get; set; }
        /*! , unit: "none", description: crop coefficient maximal value */
        public double kcMax { get; set; }
        /*! , unit: "none", description: basal crop coefficient */
        public double kcb { get; set; }
        /*! , unit: "none", description: fraction of soil surface covered by vegetation */
        public double fc { get; set; }
        /*! , unit: "none", description: fraction of soil that is exposed and wetted in the event */
        public double few { get; set; }
        /*! , unit: "mm", description: totally evaporable water */
        public double tew { get; set; }
        /*! , unit: "mm", description: readily evaporable water */
        public double rew { get; set; }
        /*! , unit: "none", description: evaporation reduction coefficient */
        public double kr { get; set; }
        /*! , unit: "mm", description: soil evaporation coefficient */
        public double ke { get; set; }
        /*! , unit: "mm", description: drainage in evaporation layer */
        public double de { get; set; }
        /*! , unit: "mm", description: percolation from evaporation layer to deeper soil layers */
        public double dpe { get; set; }
        /*! , unit: "mm", description: Calculated E value. */
        public double e { get; set; }
    }

    public class Evaporation
    {
        //calculate evaporation
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
            double autoNetIrrigationTc,
            double interception,
            double interceptionIrr,
            double interceptionAutoIrrTc,
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
            if ((netIrrigation > 0 || autoNetIrrigationTc > 0) && (irrigationType.name == "drip"))
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
            var netPrecititationTc = (double)climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigationTc - interceptionAutoIrrTc;
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
