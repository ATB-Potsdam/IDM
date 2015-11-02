/*!
 * \file    Transpiration.cs
 *
 * \brief   The irrigation module implementation.
 *
 * \author  Hunstock
 * \date    05.08.2015
 *
 * \mainpage    ATB Irrigation module overview
 *
 * \section     intro_sec Introduction to the irrigation module
 *
 * This module is intended to be used with the Microsoft .NET framework version 4.5. A version for .NET 4.0 is also available.
 *
 * \section     install_sec Installation
 *              All you need is the dynamic link library "ATB_Irrigation-Module_cs.dll".
 *              Data for a lot of plants and some standard soil types is compiled in and you can use it out of the box.
 *              Download the most recent version from here:
 *              <a href="ATB_Irrigation-Module_cs.dll">http://www.runlevel3.de/ATB_Irrigation-Module_cs.dll</a>
 *              A comprehensive documentation is available here:
 *              <a href="ATB_Irrigation-Module_docs.zip">http://www.runlevel3.de/ATB_Irrigation-Module_docs.zip</a>
 *
 * \section     quick_sec Quick Start
 *
 * \subsection  step1 Install a development environment
 *              We recommend "Visual Studio 2013 Community Edition" or the newer 2015 version, it is free but with registration. The .NET frameworks 4.0 and 4.5 are included.
 * \subsection  step2 Create a new project
 *              In the menu FILE choose "New" -> "Project".
 * \subsection  step3 Add the dll to the project
 *              In the solution explorer right click on the project and choose "Properties". Go to "References" and add the dll.
 * \subsection  step4 Use functions from the dll
 *              All exported functions are in the namespace "atbApi". Now is the right time to take a look at the documentation with a lot of code examples.
 * \subsection  step5 Compile and test your project
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using atbApi;
using atbApi.data;
using local;

/*! 
 * \brief   namespace for all exported classes and structures
 * 
 */
namespace atbApi
{

    public enum CalculationType
    {
        /*! evaporation and transpiration calc */
        e = 0x0,
        /*! evapotranspiration calc */
        et = 0x1,
    };

    /*!
     * \brief   Encapsulates the result of a transpiration or evapotranspiration calculation.
     *
     */

    public class ETResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public double runtimeMs { get; set; }
        /*! runtimeMs, unit: "none", description: If an error occured during the calculation, this value is not null and contains an error description. */
        public String? error { get; set; }
        /*! management, unit: "none", description: Dataset with irrigation events. */
        public IrrigationSchedule irrigationSchedule { get; set; }
        /*! lastConditions, unit: "none", description: Soil water balance at end of calculation for root zone and deep zone for both calculation approaches E plus T and ET. */
        public SoilConditions lastConditions { get; set; }
        /*! et0[mm], unit: "mm", description: Sum of reference Evapotranspiration for calculation period. */
        public double et0 { get; set; }
        /*! e[mm], unit: "mm", description: Sum of Evaporation for calculation period. */
        public double e { get; set; }
        /*! eAct[mm], unit: "mm", description: Sum of actual Evaporation for calculation period. */
        public double eAct { get; set; }
        /*! tc[mm], unit: "mm", description: Sum of Transpiration for calculation period. */
        public double t { get; set; }
        /*! tAct[mm], unit: "mm", description: Sum of actual Transpiration for calculation period. */
        public double tAct { get; set; }
        /*! etc[mm], unit: "mm", description: Sum of Evapotranspiration and transpiration for calculation period. */
        public double eActPlusTact { get; set; }
        /*! etc[mm], unit: "mm", description: Sum of Evapotranspiration for calculation period. */
        public double et { get; set; }
        /*! etAct[mm], unit: "mm", description: Sum of actual Evapotranspiration for calculation period. */
        public double etAct { get; set; }
        /*! precipitation[mm], unit: "mm", description: Precipitation sum for calculation period. */
        public double precipitation { get; set; }
        /*! interception[mm], unit: "mm", description: Interception sum for calculation period. This amount of water from "precipitation" is intercepted by leafes and does not reach the soil surface. */
        public double interception { get; set; }
        /*! irrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double irrigationFw { get; set; }
        /*! irrigation[mm], unit: "mm", description: Irrigation sum for calculation period. */
        public double irrigation { get; set; }
        /*! netIrrigation[mm], unit: "mm", description: Netto irrigation sum for calculation period. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double netIrrigation { get; set; }
        /*! interceptionIrr[mm], unit: "mm", description: Additional interception for the irrigated water. */
        public double interceptionIrr { get; set; }
        /*! dpRzTc[mm], unit: "mm", description: Deep percolation from root zone for dual calculation E plus T. This amount of water percolates from the root to the deep zone. */
        public double dpRz { get; set; }
        /*! dpDzTc[mm], unit: "mm", description: Deep percolation from deep zone for dual calculation E plus T. This amount of water percolates from the deep zone to ground water. */
        public double dpDz { get; set; }
        /*! drDiffTc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiff { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double autoIrrigationFw { get; set; }
        /*! autoIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Fraction of wetted surface is not considered. */
        public double autoIrrigation { get; set; }
        /*! autoNetIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigation { get; set; }
        /*! interceptionAutoIrrTc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrr { get; set; }
        /*! balanceErrorTc[mm], unit: "mm", description: Overall balance error for E plus T calculation between input water and output. The equation is: "precipitation + netIrrigation + drDiffTc - interception - interceptionIrr - e - tAct - dpDzTc + autoNetIrrigationTc - interceptionAutoIrrTc". This value must be near zero. If larger than 1E-3 the balance has gaps. */
        public double balanceError { get; set; }
        /*! kcIni, unit: "none", description: Result from kcIni model, calculated crop coefficient for initial plant stage. */
        public double kcIni { get; set; }
        /*! ksMeanTcInitial, unit: "none", description: Mean value for water stress factor "Ks" in initial stage of plant growing for E plus T calculation. */
        public MeanValue ksMeanInitial { get; set; }
        /*! ksMeanTcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for E plus T calculation. */
        public MeanValue ksMeanDevelopment { get; set; }
        /*! ksMeanTcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for E plus T calculation. */
        public MeanValue ksMeanMid_season { get; set; }
        /*! ksMeanTcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for E plus T calculation. */
        public MeanValue ksMeanLate_season { get; set; }
        /*! yieldReductionTc, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public double yieldReduction { get; set; }
        /*! yieldReductionTcInitial, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public double yieldReductionInitial { get; set; }
        /*! yieldReductionTcDevelopment, unit: "none", description: Yield reduction factor due to water stress in development stage of plant growing for E plus T calculation. */
        public double yieldReductionDevelopment { get; set; }
        /*! yieldReductionTcMid_season, unit: "none", description: Yield reduction factor due to water stress in mid_season stage of plant growing for E plus T calculation. */
        public double yieldReductionMid_season { get; set; }
        /*! yieldReductionTcLate_season, unit: "none", description: Yield reduction factor due to water stress in late_season stage of plant growing for E plus T calculation. */
        public double yieldReductionLate_season { get; set; }
        /*! kcIniResult, description: Intermediate values and result of the "kcIni" model. */
        public KcIniResult kcIniResult { get; set; }

        /*! dailyValues, description: Each line contains values for one day from "seedDate" to "harvestDate". This includes input values, intermediate results, evaporation, transpiration, evapotranspiration and soil water balance. */
        public IDictionary<DateTime, ETDailyValues> dailyValues { get; set; }

        public ETResult()
        {
            dailyValues = new Dictionary<DateTime, ETDailyValues>();
        }
    }

    public class ETDailyValues
    {
        /*! et0[mm], unit: "mm", description: Sum of reference Evapotranspiration for calculation period. */
        public double et0 { get; set; }
        /*! e[mm], unit: "mm", description: Sum of Evaporation for calculation period. */
        public double e { get; set; }
        /*! eAct[mm], unit: "mm", description: Sum of actual Evaporation for calculation period. */
        public double eAct { get; set; }
        /*! tc[mm], unit: "mm", description: Sum of Transpiration for calculation period. */
        public double t { get; set; }
        /*! tAct[mm], unit: "mm", description: Sum of actual Transpiration for calculation period. */
        public double tAct { get; set; }
        /*! etc[mm], unit: "mm", description: Sum of Evapotranspiration for calculation period. */
        public double et { get; set; }
        /*! etAct[mm], unit: "mm", description: Sum of actual Evapotranspiration for calculation period. */
        public double etAct { get; set; }
        /*! precipitation[mm], unit: "mm", description: Precipitation sum for calculation period. */
        public double precipitation { get; set; }
        public double netPrecipitation { get; set; }
        /*! interception[mm], unit: "mm", description: Interception sum for calculation period. This amount of water from "precipitation" is intercepted by leafes and does not reach the soil surface. */
        public double interception { get; set; }
        /*! irrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double irrigationFw { get; set; }
        /*! irrigation[mm], unit: "mm", description: Irrigation sum for calculation period. */
        public double irrigation { get; set; }
        /*! netIrrigation[mm], unit: "mm", description: Netto irrigation sum for calculation period. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double netIrrigation { get; set; }
        /*! interceptionIrr[mm], unit: "mm", description: Additional interception for the irrigated water. */
        public double interceptionIrr { get; set; }
        public double drRz { get; set; }
        public double drDz { get; set; }
        /*! dpRzTc[mm], unit: "mm", description: Deep percolation from root zone for dual calculation E plus T. This amount of water percolates from the root to the deep zone. */
        public double dpRz { get; set; }
        /*! dpDzTc[mm], unit: "mm", description: Deep percolation from deep zone for dual calculation E plus T. This amount of water percolates from the deep zone to ground water. */
        public double dpDz { get; set; }
        /*! drDiffTc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiff { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double autoIrrigationFw { get; set; }
        /*! autoIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Fraction of wetted surface is not considered. */
        public double autoIrrigation { get; set; }
        /*! autoNetIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigation { get; set; }
        /*! interceptionAutoIrrTc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrr { get; set; }
        /*! balanceErrorTc[mm], unit: "mm", description: Overall balance error for E plus T calculation between input water and output. The equation is: "precipitation + netIrrigation + drDiffTc - interception - interceptionIrr - e - tAct - dpDzTc + autoNetIrrigationTc - interceptionAutoIrrTc". This value must be near zero. If larger than 1E-3 the balance has gaps. */
        public double balanceError { get; set; }
        public int plantDay { get; set; }
        public double plantZr { get; set; }
        public String plantStage { get; set; }
        public double kc { get; set; }
        public double kcAdj { get; set; }
        public double kcb { get; set; }
        public double kcbAdj { get; set; }
        public double de { get; set; }
        public double dpe { get; set; }
        public double tawRz { get; set; }
        public double tawDz { get; set; }
        public double ks { get; set; }
        public Double? yieldReduction { get; set; }
        public double pAdj { get; set; }
        public double raw { get; set; }
        public double drRzExceeded { get; set; }
        public double drDzExceeded { get; set; }
        public double precIrrEff { get; set; }
        public double soilStorageEff { get; set; }
    }


    /*!
     * \brief   The main functions of the irrigation module. Different types of calculation are provided.
     *
     */

    public static class Transpiration
    {

        /*!
         * \brief   Transpiration calculation, overloaded version with only essential arguments
         *
         * \param   climate             The climate.
         * \param   plant               The plant.
         * \param   soil                The soil.
         * \param   location            latitude, longitude and altitude
         * \param   irrigationSchedule  data with irrigation amounts per calculation step
         * \param   seedDate            The seed date.
         * \param   harvestDate         The harvest date.
         * \param   initialConditions   Soil water content at begin of calculation. Six depletion values for root zone, deep zone and evaporation layer for two calculation approaches are maintained.
         * \param   autoIrr             Set to true to automatically irrigate.
         */
        public static bool ETCalc(
            CalculationType calcType,
            ref ETResult result,
            Climate climate,
            Plant plant,
            Soil soil,
            IrrigationSchedule irrigationSchedule,
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            DateTime start,
            DateTime end,
            SoilConditions initialConditions,
            AutoIrrigationControl autoIrr
        )
        {
            var profileStart = DateTime.Now;

            if (result == null) result = new ETResult();
            return ETCalc(calcType, ref result, climate, plant, soil, irrigationSchedule, location, seedDate, harvestDate, start, end, initialConditions, null, null, null, null, null, null);
        }

        /*!
         * Transpiration calculation.
         *
         * \author  Hunstock
         * \date    29.10.2015
         *
         * \param [in,out]  result      The result.
         * \param   type                The type.
         * \param   climate             The climate.
         * \param   plant               The plant.
         * \param   soil                The soil.
         * \param   irrigationSchedule  data with irrigation amounts per calculation step.
         * \param   location            latitude, longitude and altitude.
         * \param   seedDate            The seed date.
         * \param   harvestDate         The harvest date.
         * \param   start
         * Soil water content at begin of calculation. Six depletion values
         * for root zone, deep zone and evaporation layer for two
         * calculation approaches are maintained.
         * \param   end                 The end Date/Time.
         * \param   lastConditions      The last conditions.
         * \param   _as
         * Regression coefficient for calculation of global radiation. If
         * omitted, FAO recommended default value of 0.25 is used.
         * \param   _bs
         * Regression coefficient for calculation of global radiation. If
         * omitted, FAO recommended default value of 0.5 is used.
         * \param   ct
         * May be null, default: 17.8, Empirical temperature coefficient, if
         * omitted, recommended default value of 17.8 (Hargreaves 1994) is
         * used.
         * \param   ch
         * May be null, default: 0.0023, Empirical Hargreaves coefficient,
         * if omitted, recommended default value of 0.0023 (Hargreaves 1994)
         * is used.
         * \param   eh
         * May be null, default: 0.5, Empirical hargreaves exponent, if
         * omitted, recommended default value of 0.5 (Hargreaves 1994) is
         * used.
         * \param   autoIrr             Set to true to automatically irrigate.
         * \param   _autoIrrLevel
         * If "autoIrr" is true, it will start at this fraction of available
         * soil water. The value 0.8 is for 80%.
         * \param   _autoIrrCutoff
         * If "autoIrr" is true _and_ "autoIrrAmount" is 0 (automatic amount
         * calculation), then the amount is calculated to saturate the soil
         * right up to "autoIrrCutoff" value.
         * \param   _autoIrrAmount
         * If "autoIrr" is true, this amount of irrigation is added per day,
         * if available soil water drops below "autoIrrLevel". If this value
         * is 0, then the amount of drainage from last day is added if
         * available soil water drops below "autoIrrLevel".
         * \param   autoIrrType
         * If "autoIrr" is true, this type of irrigation system is used for
         * calculation of interception and fraction wetted.
         * \param   autoIrrStartDay
         * If "autoIrr" is used, irrigation is started at this day of plant
         * development. No automatic irrigation is added before this day.
         * \param   autoIrrEndDay
         * If "autoIrr" is used, irrigation ends at this day of plant
         * development. No automatic irrigation is added after this day.
         * \param   _eFactor
         * The calculated value of evaporation is always multiplied by this
         * factor to reduce evaporation because of e.g. mulching.
         * \param   _a
         * Factor for calculation of interception. The LAI (leaf area index)
         * value o the plant parameters is multiplied by this factor before
         * interception is calculated.
         * \param   kcIni
         * Crop coefficient for initial plant stage. If this argument is
         * given, the calculation of kcIni is skipped and this value is used
         * instead.
         */

        public static bool ETCalc(
            CalculationType type,
            ref ETResult result,
            Climate climate,
            Plant plant,
            Soil soil,
            IrrigationSchedule irrigationSchedule,
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            DateTime start,
            DateTime end,
            SoilConditions lastConditions,
            AutoIrrigationControl autoIrr,
            Et0PmArgs et0PmArgs,
            Et0HgArgs et0HgArgs,
            Double? _eFactor,
            Double? _a,
            Double? kcIni
        )
        {
            //plausibility checks
            if (start < seedDate) start = seedDate;
            if (end > harvestDate) end = harvestDate;
            if (seedDate >= harvestDate)
            {
                result.error = "CommonCalc: seedDate is greater than or equal to harvestDate: seedDate: " + seedDate + " harvestDate: " + harvestDate;
                return false;
            }

            //fill variables
            var eFactor = _eFactor != null ? (double)_eFactor : 1;
            var a = _a != null ? (double)_a : 0.25;
            var maxDepth = Math.Min(1.999999999999, soil.maxDepth);
            var soilSetMax = soil.getValues(maxDepth);
            var tawMax= 1000 * (soilSetMax.Qfc - soilSetMax.Qwp) * maxDepth;
            var stageTotal = plant.stageTotal;
            if (DateTime.IsLeapYear(seedDate.Year) && seedDate.DayOfYear < 61) {
                stageTotal += 1;
            } else if (DateTime.IsLeapYear(harvestDate.Year) && harvestDate.DayOfYear > 59) {
                stageTotal += 1;
            }

            var plantDayStart = plant.getPlantDay(start, seedDate, harvestDate);
            if (plantDayStart == null)
            {
                result.error = "TranspirationCalc: cannot calculate plantDayStart for this date: " + start + " seedDate: " + seedDate + " harvestDate: " + harvestDate;
                return false;
            }
            //continued calculation, check, if plant zr changed for the day before
            if (!start.Equals(seedDate) && plantDayStart > 1) plantDayStart--;
            var plantSetStart = plant.getValues(plantDayStart);

            //no initial conditions, create new, use first day plant zr
            if (lastConditions.tawRz == null)
            {
                lastConditions = new SoilConditions(soil, plantSetStart.Zr, null, null, null);
            }

            //adjust soil water balance for moved root zone
            if ((double)plantSetStart.Zr != lastConditions.zr)
            {
                var soilSetZr = soil.getValues((double)plantSetStart.Zr);
                //taw in root zone
                var tawRzZr= 1000 * (soilSetZr.Qfc - soilSetZr.Qwp) * (double)plantSetStart.Zr;
                var tawDzZr= tawMax - tawRzZr;
                SoilConditionTools.AdjustSoilConditionsZr(ref lastConditions, tawRzZr, tawDzZr, (double)plantSetStart.Zr, maxDepth);
            }
            result.drDiff = -(lastConditions.drRz + lastConditions.drDz);


            for (DateTime loopDate = start; loopDate <= end && loopDate <= harvestDate; loopDate = loopDate.AddDays(1))
            {
                var loopResult = new ETDailyValues();
                result.dailyValues.Add(loopDate, loopResult);

                var climateSet = climate.getValues(loopDate);
                var plantDay = plant.getPlantDay(loopDate, seedDate, harvestDate);
                if (plantDay == null) continue;
                var plantSet = plant.getValues(plantDay);
                var stageName = Enum.GetName(plantSet.stage.GetType(), plantSet.stage.Value);
                var soilSet = soil.getValues(Math.Min(lastConditions.et.zr, maxDepth));
                loopResult.et0 = Et0.Et0Calc(climate, loopDate, location, _as, _bs, ct, ch, eh).et0;

                loopResult.irrigation= 0.0;
                loopResult.netIrrigation= 0.0;
                loopResult.irrigationType = irrigationSchedule.irrigationType;
                loopResult.irrigationFw= irrigationType.fw;
                result.irrigationFw= irrigationFw;
                var autoIrrigationTc= 0.0;
                var autoNetIrrigationTc= 0.0;
                var autoIrrigationEtc= 0.0;
                var autoNetIrrigationEtc= 0.0;
                var autoIrrigationType= autoIrrType;
                var autoIrrigationFw= autoIrrType.fw;
                result.autoIrrigationFw = autoIrrigationFw;

                //common calculations for Tc and ETc
                var zr= Math.Min(maxDepth, (double)plantSet.Zr);
                //taw in root zone
                var tawRz= 1000 * (soilSet.Qfc - soilSet.Qwp) * zr;
                //taw in deep zone
                var tawDz= tawMax - tawRz;
                var ze= soilSet.Ze != null ? soilSet.Ze : 0.1;
                var tew= 1000 * (soilSet.Qfc - 0.5 * soilSet.Qwp) * ze;
                var cf= (1 - Math.Exp(-(double)plantSet.LAI * 0.385));

                if (irrigationSchedule.schedule.ContainsKey(loopDate)) {
                    irrigationSchedule.schedule.TryGetValue(loopDate, out irrigation);
                    netIrrigation= irrigation * irrigationFw;
                    loopResult.irrigation= irrigation;
                    loopResult.irrigationFw= irrigationFw;
                    loopResult.netIrrigation= netIrrigation;
                }

                //calculate auto irrigation
                var soilSaturationTc= tawRz != 0 ? (tawRz - lastConditions.e.drRz) / tawRz : 0;
                var soilSaturationEtc= tawRz != 0 ? (tawRz - lastConditions.et.drRz) / tawRz : 0;
                var autoIrrWindow= true;
                if ((autoIrrStartDay != null && plantDay < autoIrrStartDay) || (autoIrrEndDay != null && plantDay > autoIrrEndDay)) autoIrrWindow= false;

                if (autoIrr && autoIrrWindow && !(bool)plantSet.isFallow && tawRz != 0) {
                    if (soilSaturationTc < autoIrrLevel) {
                        autoIrrigationTc= (double)autoIrrAmount;
                        autoNetIrrigationTc= autoIrrigationTc * autoIrrigationFw;
                        if (autoIrrAmount == 0) {
                            //automatic irrigate with drainage of last day multiplied by interval if no amount is given
                            //autoIrrigation= etSum * (autoIrrInterval);
                            //new: irrigate to given saturation
                            if (soilSaturationTc < autoIrrCutoff) autoNetIrrigationTc= (autoIrrCutoff - soilSaturationTc) * tawRz - netIrrigation;
                            autoIrrigationTc= autoNetIrrigationTc / autoIrrigationFw;
                        }
                    }
                    if (soilSaturationEtc < autoIrrLevel) {
                        autoNetIrrigationEtc= autoIrrAmount;
                        autoNetIrrigationEtc= autoIrrigationEtc * autoIrrigationFw;
                        if (autoIrrAmount == 0) {
                            //automatic irrigate with drainage of last day multiplied by interval if no amount is given
                            //autoIrrigation= etSum * (autoIrrInterval);
                            //new: irrigate to given saturation
                            if (soilSaturationEtc < autoIrrCutoff) autoNetIrrigationEtc= (autoIrrCutoff - soilSaturationEtc) * tawRz - netIrrigation;
                            autoIrrigationEtc= autoNetIrrigationEtc / autoIrrigationFw;
                        }
                    }
                }

                //calculate interception for irrigation for auto and data based irrigation
                var interception = 0.0;
                if (plantSet.LAI != 0 && a != 0) {
                    interception= a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * (double)climateSet.precipitation) / (a * (double)plantSet.LAI)));
                }
                var interceptionIrr= a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * netIrrigation) / (a * (double)plantSet.LAI)));
                var interceptionAutoIrrTc = a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * autoNetIrrigationTc) / (a * (double)plantSet.LAI)));
                var interceptionAutoIrrEtc = a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * autoNetIrrigationEtc) / (a * (double)plantSet.LAI)));
                interceptionIrr= interceptionIrr * irrigationType.interception;
                interceptionAutoIrrTc= interceptionAutoIrrTc * autoIrrigationType.interception;
                interceptionAutoIrrEtc= interceptionAutoIrrEtc * autoIrrigationType.interception;

                //calculate netPrecipitation for Etc/Tc
                var netPrecipitationTc = Math.Max(0, (double)climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigationTc - interceptionAutoIrrTc);
                var netPrecipitationEtc = Math.Max(0, (double)climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigationEtc - interceptionAutoIrrEtc);

                loopResult.plantDay = (int)plantDay;
                loopResult.plantZr = zr;
                loopResult.plantStage = stageName;
                loopResult.et0 = et0.et0;
                loopResult.precipitation = (double)climateSet.precipitation;
                loopResult.interception = interception;
                loopResult.interceptionIrr = interceptionIrr;
                loopResult.tawRz = tawRz;
                loopResult.tawDz = tawDz;
                loopResult.autoIrrigationEtc = autoIrrigationEtc;
                loopResult.autoNetIrrigationEtc = autoNetIrrigationEtc;
                loopResult.interceptionAutoIrrEtc = interceptionAutoIrrEtc;
                loopResult.netPrecipitationEtc = netPrecipitationEtc;
                loopResult.autoIrrigationTc = autoIrrigationTc;
                loopResult.autoNetIrrigationTc = autoNetIrrigationTc;
                loopResult.interceptionAutoIrrTc = interceptionAutoIrrTc;
                loopResult.netPrecipitationTc = netPrecipitationTc;

                var eConditions = lastConditions.e;
                var eResult = Evaporation.ECalculation(
                    ref eConditions,
                    plantSet,
                    climateSet,
                    irrigationType,
                    autoIrrigationType,
                    et0.et0,
                    eFactor,
                    tew,
                    irrigationFw,
                    autoIrrigationFw,
                    netIrrigation,
                    autoNetIrrigationTc,
                    interception,
                    interceptionIrr,
                    interceptionAutoIrrTc
                );
                lastConditions.e = eConditions;
                loopResult.e = eResult.e;
                loopResult.eAct = eResult.e;
                loopResult.de = eResult.de;
                loopResult.dpe = eResult.dpe;
                /*
                loopResult.few = eResult.few;
                loopResult.tew = eResult.tew;
                loopResult.rew = eResult.rew;
                loopResult.kr = eResult.kr;
                loopResult.ke = eResult.ke;
                loopResult.kcMax = eResult.kcMax;
                loopResult.kcMin = eResult.kcMin;
                */

                var etConditions = lastConditions.et;
                TcCalculation(
                    climateSet,
                    plantSet,
                    (double)plantSet.Kcb,
                    ref etConditions,
                    "Tc",
                    ref loopResult
                );
                lastConditions.et = etConditions;
                
                var _kcIni = plantSet.Kc;
                if (stageName == "initial") {
                    var kcIniResult = KcIni.KcIniCalc(
                        climate,
                        plant,
                        soil,
                        irrigationSchedule,
                        loopDate,
                        plant.initialEnd,
                        location,
                        lastConditions.e.de,
                        zr,
                        plantSet.isFallow == true ? false : autoIrr,
                        autoIrrLevel,
                        autoIrrCutoff,
                        autoIrrAmount,
                        autoIrrType,
                        autoIrrStartDay,
                        autoIrrEndDay,
                        eFactor
                    );
                    kcIni = kcIniResult.kcIni;
                    _kcIni = kcIni;
                }
                if (stageName == "development") {
                    //if kcIni undefined -> plant without initial phase -> return Kc from development stage
                    if (kcIni == null) {
                        _kcIni = plantSet.Kc;
                    } else {
                        kcIni = Tools.Linear_day((int)plantDay, plant.initialEnd + 1, plant.developmentEnd, (double)kcIni, (double)plantSet.Kc);
                    }
                }

                etConditions = lastConditions.et;
                TcCalculation(
                    climateSet,
                    plantSet,
                    (double)_kcIni,
                    ref etConditions,
                    "Etc",
                    ref loopResult
                );
                lastConditions.et = etConditions;

                //adjust moved zone border
                //SoilConditionTools.AdjustSoilConditionsDualZr(ref lastConditions, tawRz, tawDz, (double)plantSet.Zr, maxDepth);
                //calculate soil water balance
                loopResult.dpRzTc = Math.Max(0, netPrecipitationTc - loopResult.e - loopResult.t - lastConditions.e.drRz);
                loopResult.drRzTc = lastConditions.e.drRz - netPrecipitationTc + loopResult.e + loopResult.t + loopResult.dpRzTc;
                loopResult.dpRzEtc = Math.Max(0, netPrecipitationEtc - loopResult.et - lastConditions.et.drRz);
                loopResult.drRzEtc = lastConditions.et.drRz - netPrecipitationEtc + loopResult.et + loopResult.dpRzEtc;
                //adjust maximum drainage
                if (loopResult.drRzTc < 0)
                {
                    //negative drainage should not happen -> percolate excess water to deep zone
                    loopResult.dpRzTc -= loopResult.drRzTc;
                    loopResult.drRzTc = 0;
                }
                else if (loopResult.drRzTc > tawRz)
                {
                    //drainage exceeds taw -> adjust this day values to stay beyond this limit
                    loopResult.drRzExceededTc = loopResult.drRzTc - tawRz;
                    loopResult.drRzExceededEtc = loopResult.drRzEtc - tawRz;

                    var _eScale = loopResult.e / (loopResult.e + loopResult.tAct);
                    var _tScale = loopResult.tAct / (loopResult.e + loopResult.tAct);
                    loopResult.tAct = Math.Min(0, loopResult.tAct - loopResult.drRzExceededTc * _tScale);
                    loopResult.eAct = Math.Min(0, loopResult.e - loopResult.drRzExceededTc * _eScale);
                    loopResult.drRzTc = lastConditions.e.drRz - netPrecipitationTc + (loopResult.eAct + loopResult.tAct) + loopResult.dpRzTc;

                    loopResult.etAct = Math.Min(0, loopResult.etAct - loopResult.drRzExceededEtc);
                    loopResult.drRzEtc = lastConditions.et.drRz - netPrecipitationEtc + loopResult.etAct + loopResult.dpRzEtc;
                }


                loopResult.dpDzEtc= Math.Max(0, loopResult.dpRzEtc - lastConditions.et.drDz);
                loopResult.drDzEtc= lastConditions.et.drDz - loopResult.dpRzEtc + loopResult.dpDzEtc;

                if (loopResult.drDzEtc < 0) {
                    //negative drainage should not happen -> deep percolate excess water
                    loopResult.dpDzEtc -= loopResult.drDzEtc;
                    loopResult.drDzEtc = 0;
                }
                if (loopResult.drDzEtc > tawDz) {
                    //drainage exceeds taw should not happen in deep zone -> write exceed value to result table
                    loopResult.drDzExceededEtc = loopResult.drDzEtc - tawDz;
                    loopResult.drDzEtc= tawDz;
                }

                
                //calculate precipitation/irrigation efficiency
                if ((climateSet.precipitation + loopResult.netIrrigation + loopResult.autoNetIrrigationEtc) > 0) {
                    loopResult.precIrrEffEtc = (loopResult.netPrecipitationEtc - loopResult.dpDzEtc) / ((double)climateSet.precipitation + netIrrigation + loopResult.autoNetIrrigationEtc);

                    //calculate soil water storage efficiency
                    if (Math.Round(lastConditions.et.drRz, 5) == 0.0) {
                        loopResult.soilStorageEffEtc = 0.0;
                    }
                    else {
                        loopResult.soilStorageEffEtc = (loopResult.netPrecipitationEtc - loopResult.dpRzEtc) / lastConditions.et.drRz;
                    }
                }

                //calculate precipitation/irrigation efficiency
                if ((climateSet.precipitation + loopResult.netIrrigation + loopResult.autoNetIrrigationTc) > 0)
                {
                    loopResult.precIrrEffTc = (loopResult.netPrecipitationTc - loopResult.dpDzTc) / ((double)climateSet.precipitation + netIrrigation + loopResult.autoNetIrrigationTc);

                    //calculate soil water storage efficiency
                    if (Math.Round(lastConditions.e.drRz, 5) == 0.0)
                    {
                        loopResult.soilStorageEffTc = 0.0;
                    }
                    else
                    {
                        loopResult.soilStorageEffTc = (loopResult.netPrecipitationTc - loopResult.dpRzTc) / lastConditions.e.drRz;
                    }
                }

                loopResult.drDiffEtc = (loopResult.drRzEtc + loopResult.drDzEtc) - (lastConditions.et.drRz + lastConditions.et.drDz);
                loopResult.drDiffTc = (loopResult.drRzTc + loopResult.drDzTc) - (lastConditions.e.drRz + lastConditions.e.drDz);
                lastConditions.e.drRz = loopResult.drRzTc;
                lastConditions.e.drDz = loopResult.drDzTc;
                lastConditions.e.tawRz = tawRz;
                lastConditions.e.tawDz = tawDz;
                lastConditions.e.zr = zr;
                lastConditions.et.drRz = loopResult.drRzEtc;
                lastConditions.et.drDz = loopResult.drDzEtc;
                lastConditions.et.tawRz = tawRz;
                lastConditions.et.tawDz = tawDz;
                lastConditions.et.zr = zr;


                result.et0 += et0.et0;
                result.precipitation += (double)climateSet.precipitation;
                result.irrigation += irrigation;
                result.netIrrigation += netIrrigation;
                result.interception += interception;

                //buildBalance(result, lastConditions);
                //buildYieldReduction();

            }//for loopDate

            result.lastConditions = lastConditions;
            result.runtimeMs = ((double)DateTime.Now.Ticks - (double)profileStart.Ticks) / 10000.0;

            return result;
        }


        internal static void CommonCalculation(
            ref ETDailyValues loopResult,
            ClimateValues climateSet,
            PlantValues plantSet,
            double kc,
            ref SoilConditions lastConditions,
            String resultSuffix,
        ) {
            //Tc / Kcb calculations
            var kcAdj= kc;
            if ((loopResult.plantStage == "mid_season" || loopResult.plantStage == "late_season") && kc > 0.45) {
                kcAdj= kc + (0.04 * ((double)climateSet.windspeed - 2) - 0.004 * ((double)climateSet.humidity - 45)) * Math.Pow(((double)plantSet.height / 3), 0.3);
            }
            var etc= loopResult.et0 * kcAdj;
            var pAdj= (double)plantSet.p + 0.04 * (5 - etc);
            //0.1 < pAdj < 0.8
            pAdj= Math.Min(0.8, pAdj);
            pAdj= Math.Max(0.1, pAdj);
            var raw= loopResult.tawRz * pAdj;
            var ks= raw != 0 ? 1.0 : 0.0;
            if ((lastConditions.drRz > raw && raw != 0) && !(bool)plantSet.isFallow) {
                ks= Math.Max(0, (loopResult.tawRz - lastConditions.drRz) / (loopResult.tawRz - raw));
            }
            var etAct= etc * ks;

            if (resultSuffix == "Etc") {
                loopResult.kc= kc;
                loopResult.kcAdj= kcAdj;
                loopResult.et= etc;
                loopResult.etAct= etAct;
                loopResult.pAdjEtc= pAdj;
                loopResult.rawEtc= raw;
                loopResult.ksEtc= ks;
                if (plantSet.Ky != null)
                {
                    loopResult.yieldReductionEtc = plantSet.Ky * (1 - ks);
                    loopResult.plantKy = plantSet.Ky;
                }
            }
            else
            {
                loopResult.kcb= kc;
                loopResult.kcbAdj= kcAdj;
                loopResult.t= etc;
                loopResult.tAct= etAct;
                loopResult.pAdjTc= pAdj;
                loopResult.rawTc= raw;
                loopResult.ksTc= ks;
                if (plantSet.Ky != null)
                {
                    loopResult.yieldReductionTc = plantSet.Ky * (1 - ks);
                    loopResult.plantKy = plantSet.Ky;
                }
            }
        }
    }
}
