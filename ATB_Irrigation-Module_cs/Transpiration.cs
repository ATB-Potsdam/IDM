/*!
 * \file    Transpiration.cs
 *
 * \brief   The irrigation module implementation.
 *
 * \author  Hunstock
 * \date    05.08.2015
 *
 * \mainpage    Overview
 *
 * \section     intro_sec Introduction to the irrigation module
 *
 * This module is intended to be used with the Microsoft .NET framework version 4.5. A version for .NET 4.0 is also available.
 *
 * \section     install_sec Installation
 *              All you need is the dynamic link library "ATB_Irrigation-Module_cs.dll" and
 *              the .net extension for JSON "Newtonsoft.Json.dll".
 *              Data for a lot of plants and some standard soil types is compiled in and you can use it out of the box.
 *              Download the most recent version from here:
 *              <a href="https://www.runlevel3.de/atb/irrigation-module/">https://www.runlevel3.de/atb/irrigation-module/</a>
 *              A comprehensive documentation is available here:
 *              <a href="https://www.runlevel3.de/atb/irrigation-module-docs/">https://www.runlevel3.de/atb/irrigation-module-docs/</a>
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
using System.Threading.Tasks;

using local;
using atbApi.data;
using atbApi.tools;

/*! 
 * \brief   namespace for all exported classes and structures
 * 
 */
namespace atbApi
{
    /*!
     * \brief   class with helper functions for the transpiration and evaporation calculation
     *
     */

    public static class ETTools
    {
        /*!
         * \brief Check arguments for for an evaporation and transpiration calculation for errors.
         *
         * \param [in,out]  args    The arguments to check.
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

    /*!
     * \brief   class to hold transient values needed for every transpiration calculation
     *          calculated once at start, put into result as transient values.
     *
     */

    public class TransientValues
    {
        /*! tawRz, unit: "mm", description: Totally available water in root zone. */
        public double tawRz { get; set; }
        /*! tawDz, unit: "mm", description: Totally available water in deep zone. */
        public double tawDz { get; set; }
        /*! tawMax, unit: "mm", description: Totally available water for maximum soil depth. */
        public double tawMax { get; set; }

        /*!
         * \brief   Default constructor
         *
         */

        public TransientValues()
        {
        }

        /*!
         * \brief   Clone Constructor
         *
         * \param   tValues The instance to clone
         */

        public TransientValues(TransientValues tValues)
        {
            //use this for .net 4.0
            //foreach (PropertyInfo pi in this.GetType().GetProperties())
            //use this for .net 4.5
            foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
            {
                if (tValues.GetType().GetRuntimeProperty(pi.Name) == null) continue;

                pi.SetValue(this, tValues.GetType().GetRuntimeProperty(pi.Name).GetValue(tValues, null), null);
            }
        }
    }

    /*!
     * \brief   Encapsulates all arguments for an evaporation and transpiration calculation.
     *
     *
     * ### param    climate             The climate.
     * ### param    plant               The plant.
     * ### param    soil                The soil.
     * ### param    irrigationSchedule  
     * ### param    location            
     * ### param    seedDate            
     * ### param    harvestDate         The harvest date. \param   start.
     * ### param    end                 The end Date/Time.
     * ### param    lastConditions      The last conditions.
     */

    public class ETArgs
    {
        /*! The climate data to use. */
        public Climate climate { get; set; }
        /*! The plant parameters to use. */
        public Plant plant { get; set; }
        /*! The soil parameters. */
        public Soil soil { get; set; }
        /*! latitude, longitude and altitude */
        public Location location { get; set; }
        /*! The seed date. */
        public DateTime seedDate { get; set; }
        /*! The harvest date. */
        public DateTime harvestDate { get; set; }
        /*! If seed date to harvest date does not match exactly the cultivation length of the plant parameters, the plant parameters are stretched to match the cultivation period. */
        public bool adaptStageLength { get; set; }
        /*! Optional parameter for step wise calculation. If omitted, the whole period from seed to harvest date is calculated. */
        public DateTime? start { get; set; }
        /*! Optional parameter for step wise calculation. If omitted, the whole period from seed to harvest date is calculated. */
        public DateTime? end { get; set; }
        /*! Data with irrigation amounts per calculation step. */
        public IrrigationSchedule irrigationSchedule { get; set; }
        /*! Soil conditions from last calculation step. */
        public SoilConditions lastConditions { get; set; }
        /*! Parameters to control, hwo automatic irrigation is applied. */
        public AutoIrrigationControl autoIrr { get; set; }
        /*! Parameters to control reference evapotranspiration calculation with Penman Monteith equation. */
        public Et0PmArgs et0PmArgs { get; set; }
        /*! Parameters to control reference evapotranspiration calculation with Hargreaves equation. */
        public Et0HgArgs et0HgArgs { get; set; }
        /*! The calculated value of evaporation is always multiplied by this factor to reduce evaporation because of e.g. mulching. */
        public double eFactor { get; set; }
        /*! Factor for calculation of interception. The LAI (leaf area index) value o the plant parameters is multiplied by this factor before interception is calculated. */
        public double a { get; set; }

        /*!
         * \brief   Default Constructor with all optional arguments.
         *
         * \param   adaptStageLength    true to adapt stage length.
         * \param   irrigationSchedule  The irrigation schedule.
         * \param   lastConditions      The last soil conditions.
         * \param   autoIrr             The automatic irrigation parameters.
         * \param   et0PmArgs           The et0 Penman Monteith arguments.
         * \param   et0HgArgs           The et0 Hargreaves arguments.
         * \param   eFactor             The factor for evaporation reduction.
         * \param   a                   The a factor for interception calculation.
         */

        public ETArgs(
            bool adaptStageLength = true,
            IrrigationSchedule irrigationSchedule = null,
            SoilConditions lastConditions = null,
            AutoIrrigationControl autoIrr = null,
            Et0PmArgs et0PmArgs = null,
            Et0HgArgs et0HgArgs = null,
            double eFactor = 1,
            double a = 0.25
        )
        {
            this.adaptStageLength = adaptStageLength;
            this.irrigationSchedule = irrigationSchedule;
            this.lastConditions = lastConditions;
            this.autoIrr = autoIrr;
            this.et0PmArgs = et0PmArgs == null ? new Et0PmArgs() : et0PmArgs;
            this.et0HgArgs = et0HgArgs == null ? new Et0HgArgs() : et0HgArgs;
            if (et0HgArgs == null) et0HgArgs = new Et0HgArgs();
            this.eFactor = eFactor;
            this.a = a;
        }

        /*!
         * \brief   Clone constructor.
         *
         * \param   etArgs  The et arguments to clone.
         */

        public ETArgs(ETArgs etArgs)
        {
            //use this for .net 4.0
            //foreach (PropertyInfo pi in this.GetType().GetProperties())
            //use this for .net 4.5
            foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
            {
                if (etArgs.GetType().GetRuntimeProperty(pi.Name) == null) continue;
                pi.SetValue(this, etArgs.GetType().GetRuntimeProperty(pi.Name).GetValue(etArgs, null), null);
            }
        }
    }


    /*!
     * \brief   Encapsulates the cumulated result of a transpiration and evaporation calculation.
     *
     */

    public class ETResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public double runtimeMs { get; set; }
        /*! error, unit: "none", description: If an error occured during the calculation, this value is not null and contains an error description. Errors are cumulated in this string.*/
        public String error { get; set; }
        /*! irrigationSchedule, unit: "none", description: Dataset with irrigation events. */
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
        /*! eActPlusTact[mm], unit: "mm", description: Sum of Evapotranspiration and transpiration for calculation period. */
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
        /*! dpRz[mm], unit: "mm", description: Deep percolation from root zone. This amount of water percolates from the root to the deep zone. */
        public double dpRz { get; set; }
        /*! dpDz[mm], unit: "mm", description: Deep percolation from deep zone. This amount of water percolates from the deep zone to ground water. */
        public double dpDz { get; set; }
        /*! drDiff[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiff { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double autoIrrigationFw { get; set; }
        /*! autoIrrigation[mm], unit: "mm", description: Sum of calculated irrigation demand. Fraction of wetted surface is not considered. */
        public double autoIrrigation { get; set; }
        /*! autoNetIrrigation[mm], unit: "mm", description: Sum of calculated irrigation demand. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigation { get; set; }
        /*! interceptionAutoIrr[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrr { get; set; }
        /*! ksMeanValues, unit: "none", description: Mean values for water stress factor "Ks" for all stages of plant growing for E plus T calculation. */
        public IDictionary<PlantStage, MeanValue> ksMeanValues { get; set; }
        /*! yieldReduction, unit: "none", description: Total yield reduction due to water stress for the whole plant growing period. */
        public double yieldReduction { get; set; }
        /*! yieldReductionValues, unit: "none", description: Yield reduction factor due to water for all stages of plant growing for E plus T calculation. */
        public IDictionary<PlantStage, double> yieldReductionValues { get; set; }
        /*! dailyValues, description: Each line contains values for one day from "seedDate" to "harvestDate". This includes input values, intermediate results, evaporation, transpiration, evapotranspiration and soil water balance. */
        public IDictionary<DateTime, ETDailyValues> dailyValues { get; set; }
        /*! tValues, unit: "none", description: Values calculated once and available for following calculations. */
        public TransientValues tValues { get; set; }

        /*!
         * \brief   Default constructor, internal dictionarys are intialized.
         *
         */

        public ETResult()
        {
            dailyValues = new Dictionary<DateTime, ETDailyValues>();
            tValues = new TransientValues();
            ksMeanValues = new Dictionary<PlantStage, MeanValue>();
            yieldReductionValues = new Dictionary<PlantStage, double>();
            irrigationFw = 1;
            autoIrrigationFw = 1;
        }

        /*!
         * \brief   Clone constructor.
         *
         * \param   etResult    The et result to clone.
         */

        public ETResult(ETResult etResult)
        {
            //use this for .net 4.0
            //foreach (PropertyInfo pi in this.GetType().GetProperties())
            //use this for .net 4.5
            foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
            {
                if (etResult.GetType().GetRuntimeProperty(pi.Name).GetValue(etResult, null) == null) continue;

                if (pi.Name == "lastConditions")
                {
                    pi.SetValue(this, new SoilConditions((SoilConditions)etResult.GetType().GetRuntimeProperty(pi.Name).GetValue(etResult, null)), null);
                }
                else if (pi.Name == "irrigationSchedule")
                {
                    pi.SetValue(this, new IrrigationSchedule(((IrrigationSchedule)etResult.GetType().GetRuntimeProperty(pi.Name).GetValue(etResult, null)).type), null);
                }
                else if (pi.Name == "ksMeanValues")
                {
                    pi.SetValue(this, new Dictionary<PlantStage, MeanValue>(), null);
                }
                else if (pi.Name == "yieldReductionValues")
                {
                    pi.SetValue(this, new Dictionary<PlantStage, double>(), null);
                }
                else if (pi.Name == "dailyValues")
                {
                    pi.SetValue(this, new Dictionary<DateTime, ETDailyValues>(), null);
                }
                else if (pi.Name == "tValues")
                {
                    pi.SetValue(this, new TransientValues((TransientValues)etResult.GetType().GetRuntimeProperty(pi.Name).GetValue(etResult, null)), null);
                }
                else {
                    pi.SetValue(this, etResult.GetType().GetRuntimeProperty(pi.Name).GetValue(etResult, null), null);
                }
            }
        }
    }

    /*!
     * \brief   Encapsulates the daily result of a transpiration and evaporation calculation.
     *
     */

    public class ETDailyValues
    {
        /*! et0[mm], unit: "mm", description: Sum of reference Evapotranspiration for calculation period. */
        public double et0 { get; set; }
        /*! e[mm], unit: "mm", description: Sum of Evaporation for calculation period. */
        public double e { get; set; }
        /*! eAct[mm], unit: "mm", description: Sum of actual Evaporation for calculation period. */
        public double eAct { get; set; }
        /*! t[mm], unit: "mm", description: Sum of Transpiration for calculation period. */
        public double t { get; set; }
        /*! tAct[mm], unit: "mm", description: Sum of actual Transpiration for calculation period. */
        public double tAct { get; set; }
        /*! precipitation[mm], unit: "mm", description: Precipitation sum for calculation period. */
        public double precipitation { get; set; }
        /*! netPrecipitation[mm], unit: "mm", description: Netto precipitation sum for calculation period. */
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
        /*! drRz[mm], unit: "mm", description: Drainage in root zone. This amount of water is actually depleted. */
        public double drRz { get; set; }
        /*! drDz[mm], unit: "mm", description: Drainage in zone from roots to maximum soil depth, usually 2m. This amount of water is actually depleted. */
        public double drDz { get; set; }
        /*! dpRz[mm], unit: "mm", description: Deep percolation from root zone. This amount of water percolates from the root to the deep zone. */
        public double dpRz { get; set; }
        /*! dpDz[mm], unit: "mm", description: Deep percolation from deep zone. This amount of water percolates from the deep zone to ground water. */
        public double dpDz { get; set; }
        /*! drDiff[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiff { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double autoIrrigationFw { get; set; }
        /*! autoIrrigation[mm], unit: "mm", description: Sum of calculated irrigation demand. Fraction of wetted surface is not considered. */
        public double autoIrrigation { get; set; }
        /*! autoNetIrrigation[mm], unit: "mm", description: Sum of calculated irrigation demand. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigation { get; set; }
        /*! interceptionAutoIrr[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrr { get; set; }
        /*! plantDay, unit: "none", description: Actual day of plant development. */
        public int plantDay { get; set; }
        /*! plantZr[m], unit: "m", description: Actual plant rooting depth. */
        public double plantZr { get; set; }
        /*! plantStage, unit: "none", description: Actual stage of plant development. */
        public String plantStage { get; set; }
        /*! kcb, unit: "none", description: The basal crop coefficient. */
        public double kcb { get; set; }
        /*! kcbAdj, unit: "none", description: The water stress adjusted basal crop coefficient. */
        public double kcbAdj { get; set; }
        /*! de, unit: "mm", description: Drainage in evaporation layer. */
        public double de { get; set; }
        /*! dpe, unit: "mm", description: Percolation from evaporation layer. */
        public double dpe { get; set; }
        /*! tawRz, unit: "mm", description: Totally available water in the root zone. */
        public double tawRz { get; set; }
        /*! tawDz, unit: "mm", description: Totally available water in the zone below roots up to 2m. */
        public double tawDz { get; set; }
        /*! ks, unit: "none", description: Water stress factor, 1 for no stress. */
        public double ks { get; set; }
        /*! pAdj, unit: "none", description: The plant critical deplation fraction p, adjusted. */
        public double pAdj { get; set; }
        /*! raw, unit: "mm", description: Readliy available water. */
        public double raw { get; set; }
        /*! drRzExceeded, unit: "mm", description: If drainage in root zone exceeds totally available water drRz > tawRz, this value is set and the drainage is corrected. */
        public double drRzExceeded { get; set; }
        /*! drDzExceeded, unit: "mm", description: If drainage in zone below roots exceeds totally available water drDz > tawDz, this value is set and the drainage is corrected. */
        public double drDzExceeded { get; set; }
        /*! precIrrEff, unit: "none", description: Efficiency regarding percolation of precipitation and irrigation, calculated as: (netPrecipitation - dpDz) / (precipitation + netIrrigation + autoNetIrrigation) */
        public double precIrrEff { get; set; }
        /*! soilStorageEff, unit: "none", description: Efficiency of water storage in soil root zone, calculated as: (netPrecipitation - dpRz) / lastConditions.drRz */
        public double soilStorageEff { get; set; }
        /*! eResult, unit: "none", description: Detailed result of the evaporation subcalculation. */
        public EvaporationResult eResult { get; set; }

        /*!
         * \brief   Default constructor, internal dictionarys are initialized.
         *
         */

        public ETDailyValues()
        {
            eResult = new EvaporationResult();
            irrigationFw = 1;
            autoIrrigationFw = 1;
        }
    }


    /*!
     * \brief   The main functions of the irrigation module. Different types of calculation are provided.
     *
     */

    public static class Transpiration
    {
        /*!
         * \brief   Transpiration calculation.
         *
         * \author  Hunstock
         * \date    29.10.2015
         *
         * \param [in,out]  args    Arguments.
         * \param [in,out]  result  The result.
         * \param   keepDailyValues True to keep daily values as part of the result
         *                          consumes a lot of memory for larger calculations.
         * \param   dryRun          True to dry run, used to calculate "nice to have" irrigation.
         *                          Result values are not cumulated.
         *
         * \return  true if it succeeds, false if it fails.
         */

        public static bool ETCalc(
            ref ETArgs args,
            ref ETResult result,
            bool keepDailyValues = false,
            bool dryRun = false
        )
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //result is not empty, assume this is a continued calculation, just continue loop
            if (result != null) {
                if (args.lastConditions == null && result.lastConditions != null)
                {
                    args.lastConditions = result.lastConditions;
                    //continue calculation only if there are lastConditions
                    return ETCalcLoop(ref args, ref result, stopWatch, keepDailyValues, dryRun);
                }
            }
            else
            {
                result = new ETResult();
            }

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

            return ETCalcLoop(ref args, ref result, stopWatch, keepDailyValues, dryRun);
        }

        //the main loop
        private static bool ETCalcLoop(
            ref ETArgs args,
            ref ETResult result,
            Stopwatch stopWatch,
            bool keepDailyValues = false,
            bool dryRun = false
        )
        {
            var et0Result = new Et0Result();

            for (DateTime loopDate = (DateTime)args.start; loopDate <= (DateTime)args.end; loopDate = loopDate.AddDays(1))
            {
                var plantDay = args.plant.getPlantDay(loopDate, args.seedDate, args.harvestDate);
                if (plantDay == null) continue;

                var loopResult = new ETDailyValues();
                if (keepDailyValues) result.dailyValues.Add(loopDate, loopResult);

                var climateSet = args.climate.getValues(loopDate);
                if (climateSet == null)
                {
                    result.error = "No climate data for dataset: " + args.climate.name + " at date: " + loopDate.ToString() + ", cannot continue!";
                    return false;
                }
                var plantSet = args.plant.getValues(plantDay);
                var stageName = Enum.GetName(plantSet.stage.GetType(), plantSet.stage.Value);
                var zr = Math.Min(args.soil.maxDepth, (double)plantSet.Zr);
                var soilSet = args.soil.getValues(zr);
                if (!Et0.Et0Calc(args.climate, loopDate, args.location, args.et0PmArgs, args.et0HgArgs, ref et0Result))
                {
                    result.error = "Cannot caluculate ET0 for dataset: " + args.climate.name + " at date: " + loopDate.ToString() + ", cannot continue!";
                    return false;
                }
                loopResult.et0 = (double)et0Result.et0;

                loopResult.irrigation = 0.0;
                loopResult.netIrrigation = 0.0;
                var irrigationType = args.irrigationSchedule != null ? args.irrigationSchedule.type : args.autoIrr != null ? args.autoIrr.type : IrrigationTypes.sprinkler;
                if (irrigationType != null) {
                    loopResult.irrigationFw = irrigationType.fw;
                    loopResult.autoIrrigationFw = irrigationType.fw;
                    result.autoIrrigationFw = irrigationType.fw;
                    result.irrigationFw = irrigationType.fw;
                }
                else {
                    loopResult.irrigationFw = 1;
                    loopResult.autoIrrigationFw = 1;
                    result.autoIrrigationFw = 1;
                    result.irrigationFw = 1;
                }
                loopResult.autoIrrigation = 0.0;
                loopResult.autoNetIrrigation = 0.0;

                //common calculations for Tc and ETc
                //taw in root zone
                var tawRz = 1000 * (soilSet.Qfc - soilSet.Qwp) * zr;
                //taw in deep zone
                var tawDz = result.tValues.tawMax - tawRz;
                var ze = soilSet.Ze != null ? (double)soilSet.Ze : 0.1;
                var soilSetZe = args.soil.getValues(ze);
                var tew = 1000 * (soilSetZe.Qfc - 0.5 * soilSetZe.Qwp) * ze;
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
                    loopResult.irrigationFw = args.irrigationSchedule.type != null ? args.irrigationSchedule.type.fw : 1;
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

                if (args.autoIrr != null && autoIrrWindow && !plantSet.isFallow && tawRz != 0)
                {
                    if (args.autoIrr.level == 0) {
                        if (soilSaturation < 1 - loopResult.pAdj - args.autoIrr.deficit)
                        {
                            loopResult.autoIrrigation = args.autoIrr.amount.amount;
                            loopResult.autoIrrigationFw = args.autoIrr.type.fw;
                            loopResult.autoNetIrrigation = loopResult.autoIrrigation * args.autoIrr.type.fw;
                            if (args.autoIrr.amount.amount == 0)
                            {
                                loopResult.autoNetIrrigation = (1 - loopResult.pAdj - args.autoIrr.deficit - soilSaturation + args.autoIrr.cutoff) * tawRz - loopResult.netIrrigation;
                                loopResult.autoIrrigation = loopResult.autoNetIrrigation / args.autoIrr.type.fw;
                            }
                        }
                    }
                    else {
                        if (soilSaturation < args.autoIrr.level)
                        {
                            loopResult.autoIrrigation = args.autoIrr.amount.amount;
                            loopResult.autoIrrigationFw = args.autoIrr.type.fw;
                            loopResult.autoNetIrrigation = loopResult.autoIrrigation * args.autoIrr.type.fw;
                            if (args.autoIrr.amount.amount == 0)
                            {
                                //irrigate to given saturation
                                if (soilSaturation < args.autoIrr.cutoff)
                                    loopResult.autoNetIrrigation = (args.autoIrr.cutoff - soilSaturation) * tawRz - loopResult.netIrrigation;
                                loopResult.autoIrrigation = loopResult.autoNetIrrigation / args.autoIrr.type.fw;
                            }
                        }
                    }
                        
                    if (loopResult.autoIrrigation != 0 && args.autoIrr.type.min != 0)
                    {
                        loopResult.autoIrrigation = Math.Max(loopResult.autoIrrigation, args.autoIrr.type.min);
                        loopResult.autoNetIrrigation = loopResult.autoIrrigation * args.autoIrr.type.fw;
                    }
                    if (loopResult.autoIrrigation != 0 && args.autoIrr.type.max != 0)
                    {
                        loopResult.autoIrrigation = Math.Min(loopResult.autoIrrigation, args.autoIrr.type.max);
                        loopResult.autoNetIrrigation = loopResult.autoIrrigation * args.autoIrr.type.fw;
                    }
                }

                var rpPrecipitation = climateSet.rpPrecipitation != null ? (double)climateSet.rpPrecipitation : (double)climateSet.precipitation;
                //calculate interception for irrigation for auto and data based irrigation
                loopResult.interception = 0.0;
                if (plantSet.LAI != 0 && args.a != 0)
                {
                    loopResult.interception = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * rpPrecipitation) / (args.a * (double)plantSet.LAI)));
                    loopResult.interceptionIrr = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * loopResult.netIrrigation) / (args.a * (double)plantSet.LAI)));
                    loopResult.interceptionIrr = args.irrigationSchedule != null && args.irrigationSchedule.type != null ? loopResult.interceptionIrr * args.irrigationSchedule.type.interception : 0.0;
                    loopResult.interceptionAutoIrr = args.a * (double)plantSet.LAI * (1 - 1 / (1 + (cf * loopResult.autoNetIrrigation) / (args.a * (double)plantSet.LAI)));
                    loopResult.interceptionAutoIrr = args.autoIrr != null && args.autoIrr.type != null ? loopResult.interceptionAutoIrr * args.autoIrr.type.interception : 0.0;
                }

                //calculate netPrecipitation for Etc/Tc
                loopResult.netPrecipitation =
                    Math.Max(0,
                        rpPrecipitation
                        - loopResult.interception
                        + loopResult.netIrrigation
                        - loopResult.interceptionIrr
                        + loopResult.autoNetIrrigation
                        - loopResult.interceptionAutoIrr
                    );

                loopResult.plantDay = (int)plantDay;
                loopResult.plantZr = zr;
                loopResult.plantStage = stageName;
                loopResult.precipitation = rpPrecipitation;
                loopResult.tawRz = tawRz;
                loopResult.tawDz = tawDz;
                loopResult.raw = tawRz * loopResult.pAdj;
                loopResult.ks = loopResult.raw != 0 ? 1 : 0;
                if ((args.lastConditions.drRz > loopResult.raw && loopResult.raw != 0) && !plantSet.isFallow)
                {
                    loopResult.ks = Math.Max(0, (tawRz - args.lastConditions.drRz) / (tawRz - loopResult.raw));
                }
                loopResult.tAct = loopResult.t * loopResult.ks;
                if (loopResult.tAct < 0.001) loopResult.tAct = 0;
                if (plantSet.stage != null)
                {
                    if (!result.ksMeanValues.ContainsKey((PlantStage)plantSet.stage))
                    {
                        result.ksMeanValues.Add((PlantStage)plantSet.stage, new MeanValue());
                        result.yieldReductionValues.Add((PlantStage)plantSet.stage, 0.0);
                    }
                    result.ksMeanValues[(PlantStage)plantSet.stage].Add(loopResult.ks);
                    if (plantSet.Ky != null) {
                        result.ksMeanValues[(PlantStage)plantSet.stage].Add(loopResult.ks);
                        result.yieldReductionValues[(PlantStage)plantSet.stage] = (double)plantSet.Ky * (1 - result.ksMeanValues[(PlantStage)plantSet.stage].value);
                    }
                }

                var eConditions = args.lastConditions;
                var eResult = loopResult.eResult;
                Evaporation.ECalculation(
                    ref eConditions,
                    plantSet,
                    climateSet,
                    irrigationType != null ? irrigationType.name : "sprinkler",
                    loopResult.et0,
                    args.eFactor,
                    tew,
                    loopResult.irrigationFw,
                    loopResult.autoIrrigationFw,
                    loopResult.netIrrigation,
                    loopResult.autoNetIrrigation,
                    loopResult.interception,
                    loopResult.interceptionIrr,
                    loopResult.interceptionAutoIrr,
                    ref eResult
                );
                args.lastConditions = eConditions;
                loopResult.eResult = eResult;
                if (eResult.e < 0.001) eResult.e = 0;
                loopResult.e = eResult.e;
                loopResult.eAct = eResult.e;
                loopResult.de = eResult.de;
                loopResult.dpe = eResult.dpe;

                //adjust moved zone border
                //moved border rootZone -> adjust drainage
                if (Math.Min((double)plantSet.Zr, args.soil.maxDepth) != args.lastConditions.zr)
                {
                    var dzDepth = args.soil.maxDepth - args.lastConditions.zr;
                    var zDiff = (double)plantSet.Zr - args.lastConditions.zr;
                    var drDiff = zDiff * args.lastConditions.drDz / dzDepth;
                    args.lastConditions.drRz += drDiff;
                    args.lastConditions.drDz -= drDiff;
                }

                //args.lastConditions = SoilConditionTools.AdjustSoilConditionsZr(args.lastConditions, tawRz, tawDz, (double)plantSet.Zr, args.soil.maxDepth);
                //calculate soil water balance
                loopResult.dpRz = Math.Max(0, loopResult.netPrecipitation - loopResult.e - loopResult.t - args.lastConditions.drRz);
                loopResult.drRz = args.lastConditions.drRz - loopResult.netPrecipitation + loopResult.e + loopResult.tAct + loopResult.dpRz;
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

                    if ((loopResult.eAct + loopResult.tAct) > 0.001)
                    {
                        var _eScale = loopResult.e / (loopResult.e + loopResult.tAct);
                        var _tScale = loopResult.tAct / (loopResult.e + loopResult.tAct);
                        loopResult.tAct = Math.Max(0, loopResult.tAct - loopResult.drRzExceeded * _tScale);
                        loopResult.eAct = Math.Max(0, loopResult.e - loopResult.drRzExceeded * _eScale);
                    }
                    else {
                        loopResult.tAct= 0;
                        loopResult.eAct= 0;
                    }
                    loopResult.drRz = args.lastConditions.drRz - loopResult.netPrecipitation + (loopResult.eAct + loopResult.tAct) + loopResult.dpRz;
                }

                loopResult.dpDz = Math.Max(0, loopResult.dpRz - args.lastConditions.drDz);
                loopResult.drDz = args.lastConditions.drDz - loopResult.dpRz + loopResult.dpDz;

                if (loopResult.drDz < 0)
                {
                    //negative drainage should not happen -> deep percolate excess water
                    loopResult.dpDz -= loopResult.drDz;
                    loopResult.drDz = 0;
                }
                if (loopResult.drDz > tawDz)
                {
                    //drainage exceeds taw should not happen in deep zone -> write exceed value to result table
                    loopResult.drDzExceeded = loopResult.drDz - tawDz;
                    loopResult.drDz = tawDz;
                }

                //calculate precipitation/irrigation efficiency
                if ((rpPrecipitation + loopResult.netIrrigation + loopResult.autoNetIrrigation) > 0)
                {
                    loopResult.precIrrEff = (loopResult.netPrecipitation - loopResult.dpDz) / (rpPrecipitation + loopResult.netIrrigation + loopResult.autoNetIrrigation);

                    //calculate soil water storage efficiency
                    if (Math.Round(args.lastConditions.drRz, 5, MidpointRounding.AwayFromZero) == 0.0)
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
            }//for loopDate

            //reset yieldReduction from last (monthly) runs and build new sum
            result.yieldReduction = 1;
            foreach (PlantStage stage in result.ksMeanValues.Keys) {
                result.yieldReduction *= 1 - result.yieldReductionValues[stage];
            }
            result.yieldReduction = 1 - result.yieldReduction;
            result.lastConditions = args.lastConditions;
            result.runtimeMs += stopWatch.Elapsed.TotalMilliseconds;

            return true;
        }

    }
}
