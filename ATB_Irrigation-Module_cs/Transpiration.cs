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
using System.Reflection;

using atbApi;
using atbApi.data;
using local;
using System.Threading.Tasks;

/*! 
 * \brief   namespace for all exported classes and structures
 * 
 */
namespace atbApi
{
    public static class ETTools
    {
        /*!
         * \brief Check ET arguments.
         *
         * \param [in,out]  args    The arguments.
         *
         * \return  A String on error, null on success.
         */

        public static String CheckArgs(ref ETArgs args)
        {
            //plausibility checks
            if (args.climate == null) return "ETTools.CheckArgs: climate is required, but null";
            if (args.plant == null) return "ETTools.CheckArgs: plant is required, but null";
            if (args.soil == null) return "ETTools.CheckArgs: soil is required, but null";
            if (args.seedDate == null) return "ETTools.CheckArgs: seedDate is required, but null";
            if (args.harvestDate == null) return "ETTools.CheckArgs: harvestDate is required, but null";
            if (args.start == null) args.start = args.seedDate;
            if (args.start < args.seedDate) args.start = args.seedDate;
            if (args.end == null) args.end = args.harvestDate;
            if (args.end > args.harvestDate) args.end = args.harvestDate;
            if (args.seedDate >= args.harvestDate)
            {
                return "ETTools.CheckArgs: seedDate is greater than or equal to harvestDate: seedDate: " + args.seedDate + " harvestDate: " + args.harvestDate;
            }
            return null;
        }

        internal static int StageTotalLeapYear(Plant plant, DateTime seedDate, DateTime harvestDate)
        {
            var stageTotal = plant.stageTotal;
            if (DateTime.IsLeapYear(seedDate.Year) && seedDate.DayOfYear < 61)
            {
                stageTotal += 1;
            }
            else if (DateTime.IsLeapYear(harvestDate.Year) && harvestDate.DayOfYear > 59)
            {
                stageTotal += 1;
            }

            return stageTotal;
        }

        internal static void CumulateResults(ref ETResult result, ref ETDailyValues loopResult)
        {
            result.et0 += loopResult.et0;
            result.e += loopResult.e;
            result.eAct += loopResult.eAct;
            result.t += loopResult.t;
            result.tAct += loopResult.tAct;
            result.eActPlusTact += loopResult.eAct + loopResult.tAct;
            result.precipitation += loopResult.precipitation;
            result.interception += loopResult.interception;
            result.irrigationFw = loopResult.irrigationFw;
            result.irrigation += loopResult.irrigation;
            result.netIrrigation += loopResult.netIrrigation;
            result.interceptionIrr += loopResult.interceptionIrr;
            result.dpRz += loopResult.dpRz;
            result.dpDz += loopResult.dpDz;
            result.drDiff += loopResult.drDiff;
            result.autoIrrigationFw = loopResult.autoIrrigationFw;
            result.autoIrrigation += loopResult.autoIrrigation;
            result.autoNetIrrigation += loopResult.autoNetIrrigation;
            result.interceptionAutoIrr += loopResult.interceptionAutoIrr;
        }
    }

    //calculated once at start, put into result as transient values
    internal class TransientValues
    {
        public double tawRz { get; set; }
        public double tawDz { get; set; }
        public double tawMax { get; set; }
    }

    /*!
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
    public class ETArgs
    {
        public Climate climate { get; set; }
        public Plant plant { get; set; }
        public Soil soil { get; set; }
        public Location location { get; set; }
        public DateTime seedDate { get; set; }
        public DateTime harvestDate { get; set; }
        public bool adaptStageLength { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        //from here optional arguments
        public IrrigationSchedule irrigationSchedule { get; set; }
        public SoilConditions lastConditions { get; set; }
        public AutoIrrigationControl autoIrr { get; set; }
        public Et0PmArgs et0PmArgs { get; set; }
        public Et0HgArgs et0HgArgs { get; set; }
        public double eFactor { get; set; }
        public double a { get; set; }

        public ETArgs()
        {
            adaptStageLength = true;
            //from here optional arguments
            irrigationSchedule = null;
            lastConditions = null;
            autoIrr = null;
            et0PmArgs = new Et0PmArgs();
            et0HgArgs = new Et0HgArgs();
            eFactor = 1;
            a = 0.25;
        }

        //clone constructor
        public ETArgs(ETArgs etArgs)
        {
            //use this for .net 4.0
            //foreach (PropertyInfo pi in this.GetType().GetProperties())
            //use this for .net 4.5
            foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
            {
                pi.SetValue(this, etArgs.GetType().GetRuntimeProperty(pi.Name).GetValue(etArgs, null), null);
            }
        }
    }


    /*!
     * \brief   Encapsulates the result of a transpiration or evapotranspiration calculation.
     *
     */

    public class ETResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public double runtimeMs { get; set; }
        /*! runtimeMs, unit: "none", description: If an error occured during the calculation, this value is not null and contains an error description. */
        public String error { get; set; }
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
//        public MeanValue ksMeanInitial { get; set; }
        /*! ksMeanTcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for E plus T calculation. */
//        public MeanValue ksMeanDevelopment { get; set; }
        /*! ksMeanTcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for E plus T calculation. */
//        public MeanValue ksMeanMid_season { get; set; }
        /*! ksMeanTcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for E plus T calculation. */
//        public MeanValue ksMeanLate_season { get; set; }
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

        /*! dailyValues, description: Each line contains values for one day from "seedDate" to "harvestDate". This includes input values, intermediate results, evaporation, transpiration, evapotranspiration and soil water balance. */
        public IDictionary<DateTime, ETDailyValues> dailyValues { get; set; }
        internal TransientValues tValues { get; set; }

        public ETResult()
        {
            dailyValues = new Dictionary<DateTime, ETDailyValues>();
            tValues = new TransientValues();
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
        public EvaporationResult eResult { get; set; }
    }


    /*!
     * \brief   The main functions of the irrigation module. Different types of calculation are provided.
     *
     */

    public static class Transpiration
    {

        /*!
         * Transpiration calculation.
         *
         * \author  Hunstock
         * \date    29.10.2015
         *
         * \param [in,out]  args        Arguments.
         * \param [in,out]  result      The result.
         */

        public static bool ETCalc(
            ref ETArgs args,
            ref ETResult result
        )
        {
            var profileStart = DateTime.Now;
            //result is not empty, assume this is a continued calculation, just continue loop
            if (result != null) return ETCalcLoop(ref args, ref result, profileStart);

            result = new ETResult();
            result.error = ETTools.CheckArgs(ref args);
            if (result.error != null) return false;

            //fill variables
            var plantSetStart = args.plant.getValues(1);
            if (plantSetStart == null || !plantSetStart.Zr.HasValue)
            {
                result.error = "ETTools: cannot read plant data for day 1, cannot continue!";
                return false;
            }

            var soilSetZr = args.soil.getValues((double)plantSetStart.Zr);
            var soilSetMax = args.soil.getValues(args.soil.maxDepth);

            result.tValues.tawRz = 1000 * (soilSetZr.Qfc - soilSetZr.Qwp) * Math.Min((double)plantSetStart.Zr, args.soil.maxDepth);
            result.tValues.tawMax = 1000 * (soilSetMax.Qfc - soilSetMax.Qwp) * args.soil.maxDepth;
            //taw in deep zone
            result.tValues.tawDz = result.tValues.tawMax - result.tValues.tawRz;

            //no initial conditions, create new, use first day plant zr
            //FIXME: add initial depletion to args
            if (args.lastConditions == null) args.lastConditions = new SoilConditions(soil: args.soil, zr: (double)plantSetStart.Zr, depletionRz: 0.1, depletionDz: 0.1, depletionDe: 0.1);

            //adjust soil water balance for moved root zone
            args.lastConditions = SoilConditionTools.AdjustSoilConditionsZr(args.lastConditions, result.tValues.tawRz, result.tValues.tawDz, (double)plantSetStart.Zr, args.soil.maxDepth);

            result.drDiff = -(args.lastConditions.drRz + args.lastConditions.drDz);

            return ETCalcLoop(ref args, ref result, profileStart);
        }


        private static bool ETCalcLoop(
            ref ETArgs args,
            ref ETResult result,
            DateTime profileStart
        )
        {
            var et0Result = new Et0Result();

            for (DateTime loopDate = (DateTime)args.start; loopDate <= (DateTime)args.end; loopDate = loopDate.AddDays(1))
            {
                var loopResult = new ETDailyValues();
                result.dailyValues.Add(loopDate, loopResult);

                var climateSet = args.climate.getValues(loopDate);
                var plantDay = args.plant.getPlantDay(loopDate, args.seedDate, args.harvestDate);
                if (plantDay == null) continue;
                var plantSet = args.plant.getValues(plantDay);
                var stageName = Enum.GetName(plantSet.stage.GetType(), plantSet.stage.Value);
                var soilSet = args.soil.getValues(Math.Min(args.lastConditions.zr, args.soil.maxDepth));
                if (!Et0.Et0Calc(args.climate, loopDate, args.location, args.et0PmArgs, args.et0HgArgs, ref et0Result)) return false;
                loopResult.et0 = (double)et0Result.et0;

                loopResult.irrigation = 0.0;
                loopResult.netIrrigation = 0.0;
                var irrigationType = args.irrigationSchedule != null ? args.irrigationSchedule.type : args.autoIrr != null ? args.autoIrr.type : IrrigationTypes.sprinkler;
                loopResult.irrigationFw = irrigationType.fw;
                result.irrigationFw = irrigationType.fw;
                loopResult.autoIrrigation = 0.0;
                loopResult.autoNetIrrigation = 0.0;

                //common calculations for Tc and ETc
                var zr = Math.Min(args.soil.maxDepth, (double)plantSet.Zr);
                //taw in root zone
                var tawRz = 1000 * (soilSet.Qfc - soilSet.Qwp) * zr;
                //taw in deep zone
                var tawDz = result.tValues.tawMax - tawRz;
                var ze = soilSet.Ze != null ? (double)soilSet.Ze : 0.1;
                var tew = 1000 * (soilSet.Qfc - 0.5 * soilSet.Qwp) * ze;
                var cf = (1 - Math.Exp(-(double)plantSet.LAI * 0.385));

                loopResult.kcbAdj= (double)plantSet.Kcb;
                if ((plantSet.stage == PlantStage.mid_season || plantSet.stage == PlantStage.late_season) && plantSet.Kcb > 0.45)
                {
                    loopResult.kcbAdj = (double)plantSet.Kcb + (0.04 * ((double)climateSet.windspeed - 2) - 0.004 * ((double)climateSet.humidity - 45)) * Math.Pow(((double)plantSet.height / 3), 0.3);
                }
                loopResult.t = loopResult.et0 * loopResult.kcbAdj;
                loopResult.pAdj = (double)plantSet.p + 0.04 * (5 - loopResult.t);
                //0.1 < pAdj < 0.8
                loopResult.pAdj = Math.Min(0.8, loopResult.pAdj);
                loopResult.pAdj = Math.Max(0.1, loopResult.pAdj);

                if (args.irrigationSchedule != null && args.irrigationSchedule.schedule != null && args.irrigationSchedule.schedule.ContainsKey(loopDate))
                {
                    double _irrigation;
                    args.irrigationSchedule.schedule.TryGetValue(loopDate, out _irrigation);
                    loopResult.irrigation = _irrigation;
                    loopResult.irrigationFw = args.irrigationSchedule.type.fw;
                    loopResult.netIrrigation = loopResult.irrigation * loopResult.irrigationFw;
                }

                //calculate auto irrigation
                var soilSaturation = tawRz != 0 ? (tawRz - args.lastConditions.drRz) / tawRz : 0;

                var autoIrrWindow = true;
                if (
                    args.autoIrr != null
                    && (
                        (args.autoIrr.startDay != null && plantDay < args.autoIrr.startDay)
                        || (args.autoIrr.endDay != null && plantDay > args.autoIrr.endDay)
                    )
                ) autoIrrWindow = false;

                if (args.autoIrr != null && autoIrrWindow && !(bool)plantSet.isFallow && tawRz != 0)
                {
                    if (soilSaturation < args.autoIrr.level)
                    {
                        loopResult.autoIrrigation = args.autoIrr.amount;
                        loopResult.autoNetIrrigation = loopResult.autoIrrigation * args.autoIrr.type.fw;
                        if (args.autoIrr.amount == 0)
                        {
                            //irrigate to given saturation
                            if (soilSaturation < args.autoIrr.cutoff)
                                loopResult.autoNetIrrigation = (args.autoIrr.cutoff - soilSaturation) * tawRz - loopResult.netIrrigation;
                            loopResult.autoIrrigation = loopResult.autoNetIrrigation / args.autoIrr.type.fw;
                        }
                    }
                }

                //calculate interception for irrigation for auto and data based irrigation
                loopResult.interception = 0.0;
                if (plantSet.LAI != 0 && args.a != 0)
                {
                    loopResult.interception = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * (double)climateSet.precipitation) / (args.a * (double)plantSet.LAI)));
                }
                loopResult.interceptionIrr = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * loopResult.netIrrigation) / (args.a * (double)plantSet.LAI)));
                loopResult.interceptionAutoIrr = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * loopResult.autoNetIrrigation) / (args.a * (double)plantSet.LAI)));
                loopResult.interceptionIrr = args.irrigationSchedule != null && args.irrigationSchedule.type != null ? loopResult.interceptionIrr * args.irrigationSchedule.type.interception : 0.0;
                loopResult.interceptionAutoIrr = args.autoIrr != null && args.autoIrr.type != null ? loopResult.interceptionAutoIrr * args.autoIrr.type.interception : 0.0;

                //calculate netPrecipitation for Etc/Tc
                loopResult.netPrecipitation =
                    Math.Max(0,
                        (double)climateSet.precipitation
                        - loopResult.interception
                        + loopResult.netIrrigation
                        - loopResult.interceptionIrr
                        + loopResult.autoNetIrrigation
                        - loopResult.interceptionAutoIrr
                    );

                loopResult.plantDay = (int)plantDay;
                loopResult.plantZr = zr;
                loopResult.plantStage = stageName;
                loopResult.precipitation = (double)climateSet.precipitation;
                loopResult.tawRz = tawRz;
                loopResult.tawDz = tawDz;


                var eConditions = args.lastConditions;
                var eResult = Evaporation.ECalculation(
                    ref eConditions,
                    plantSet,
                    climateSet,
                    irrigationType,
                    loopResult.et0,
                    args.eFactor,
                    tew,
                    loopResult.irrigationFw,
                    loopResult.autoIrrigationFw,
                    loopResult.netIrrigation,
                    loopResult.autoNetIrrigation,
                    loopResult.interception,
                    loopResult.interceptionIrr,
                    loopResult.interceptionAutoIrr
                );
                args.lastConditions = eConditions;
                loopResult.e = eResult.e;
                loopResult.eAct = eResult.e;
                loopResult.de = eResult.de;
                loopResult.dpe = eResult.dpe;
                loopResult.eResult = eResult;


                //adjust moved zone border
                //SoilConditionTools.AdjustSoilConditionsDualZr(ref lastConditions, tawRz, tawDz, (double)plantSet.Zr, maxDepth);
                //calculate soil water balance
                loopResult.dpRz = Math.Max(0, loopResult.netPrecipitation - loopResult.e - loopResult.t - args.lastConditions.drRz);
                loopResult.drRz = args.lastConditions.drRz - loopResult.netPrecipitation + loopResult.e + loopResult.t + loopResult.dpRz;
                //adjust maximum drainage
                if (loopResult.drRz < 0)
                {
                    //negative drainage should not happen -> percolate excess water to deep zone
                    loopResult.dpRz -= loopResult.drRz;
                    loopResult.drRz = 0;
                }
                else if (loopResult.drRz > tawRz)
                {
                    //drainage exceeds taw -> adjust this day values to stay beyond this limit
                    loopResult.drRzExceeded = loopResult.drRz - tawRz;
                    loopResult.drRzExceeded = loopResult.drRz - tawRz;

                    var _eScale = loopResult.e / (loopResult.e + loopResult.tAct);
                    var _tScale = loopResult.tAct / (loopResult.e + loopResult.tAct);
                    loopResult.tAct = Math.Min(0, loopResult.tAct - loopResult.drRzExceeded * _tScale);
                    loopResult.eAct = Math.Min(0, loopResult.e - loopResult.drRzExceeded * _eScale);
                    loopResult.drRz = args.lastConditions.drRz - loopResult.netPrecipitation + (loopResult.eAct + loopResult.tAct) + loopResult.dpRz;
                }


                //calculate precipitation/irrigation efficiency
                if ((climateSet.precipitation + loopResult.netIrrigation + loopResult.autoNetIrrigation) > 0)
                {
                    loopResult.precIrrEff = (loopResult.netPrecipitation - loopResult.dpDz) / ((double)climateSet.precipitation + loopResult.netIrrigation + loopResult.autoNetIrrigation);

                    //calculate soil water storage efficiency
                    if (Math.Round(args.lastConditions.drRz, 5) == 0.0)
                    {
                        loopResult.soilStorageEff = 0.0;
                    }
                    else
                    {
                        loopResult.soilStorageEff = (loopResult.netPrecipitation - loopResult.dpRz) / args.lastConditions.drRz;
                    }
                }

                loopResult.drDiff = (loopResult.drRz + loopResult.drDz) - (args.lastConditions.drRz + args.lastConditions.drDz);
                args.lastConditions.drRz = loopResult.drRz;
                args.lastConditions.drDz = loopResult.drDz;
                args.lastConditions.tawRz = tawRz;
                args.lastConditions.tawDz = tawDz;
                args.lastConditions.zr = zr;

                ETTools.CumulateResults(ref result, ref loopResult);

                //buildBalance(result, lastConditions);
                //buildYieldReduction();

            }//for loopDate

            result.lastConditions = args.lastConditions;
            result.runtimeMs = ((double)DateTime.Now.Ticks - (double)profileStart.Ticks) / 10000.0;

            return true;
        }

    }
}
