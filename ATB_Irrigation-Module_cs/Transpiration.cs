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
    /*!
     * \brief   Encapsulates the result of a transpiration calculation.
     *
     */

    public class TranspirationResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public double runtimeMs { get; set; }
        /*! location, unit: "decimal degree", description: Geographical location of the calculation. */
        public Location location { get; set; }
        /*! plant, unit: "none", description: Dataset with plant parameters. */
        public Plant plant { get; set; }
        /*! soil, unit: "none", description: Dataset with soil parameters. */
        public Soil soil { get; set; }
        /*! climate, unit: "none", description: Dataset with climate data. */
        public Climate climate { get; set; }
        /*! management, unit: "none", description: Dataset with irrigation events. */
        public IrrigationSchedule irrigationSchedule { get; set; }
        /*! seedDate, unit: "none", description: Date of crop seeding. */
        public DateTime seedDate { get; set; }
        /*! harvestDate, unit: "none", description: Date of crop harvesting. */
        public DateTime harvestDate { get; set; }
        /*! initialConditions, unit: "none", description: Soil water balance at start of calculation for root zone and deep zone for both calculation approaches E plus T and ET. */
        public SoilConditionsDual initialConditions { get; set; }
        /*! lastConditions, unit: "none", description: Soil water balance at end of calculation for root zone and deep zone for both calculation approaches E plus T and ET. */
        public SoilConditionsDual lastConditions { get; set; }
        /*! et0[mm], unit: "mm", description: Sum of reference Evapotranspiration for calculation period. */
        public double et0 { get; set; }
        /*! e[mm], unit: "mm", description: Sum of Evaporation for calculation period. */
        public double e { get; set; }
        /*! eAct[mm], unit: "mm", description: Sum of actual Evaporation for calculation period. */
        public double eAct { get; set; }
        /*! tc[mm], unit: "mm", description: Sum of Transpiration for calculation period. */
        public double tc { get; set; }
        /*! tAct[mm], unit: "mm", description: Sum of actual Transpiration for calculation period. */
        public double tAct { get; set; }
        /*! eAct+tAct[mm], unit: "mm", description: Sum of actual Evaporation and actual Transpiration for calculation period. */
        public double eActPlusTact { get; set; }
        /*! etc[mm], unit: "mm", description: Sum of Evapotranspiration for calculation period. */
        public double etc { get; set; }
        /*! etAct[mm], unit: "mm", description: Sum of actual Evapotranspiration for calculation period. */
        public double etAct { get; set; }
        /*! eAct+tAct-etAct[mm], unit: "mm", description: Balance error between the two calculation approaches E plus T and ET. If positive, "etAct" is smaller than "eAct" plus "tAct". */
        public double eActPlusTactMinusEtAct { get; set; }
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
        public double dpRzTc { get; set; }
        /*! dpDzTc[mm], unit: "mm", description: Deep percolation from deep zone for dual calculation E plus T. This amount of water percolates from the deep zone to ground water. */
        public double dpDzTc { get; set; }
        /*! drDiffTc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiffTc { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public double autoIrrigationFw { get; set; }
        /*! autoIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Fraction of wetted surface is not considered. */
        public double autoIrrigationTc { get; set; }
        /*! autoNetIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigationTc { get; set; }
        /*! interceptionAutoIrrTc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrrTc { get; set; }
        /*! balanceErrorTc[mm], unit: "mm", description: Overall balance error for E plus T calculation between input water and output. The equation is: "precipitation + netIrrigation + drDiffTc - interception - interceptionIrr - e - tAct - dpDzTc + autoNetIrrigationTc - interceptionAutoIrrTc". This value must be near zero. If larger than 1E-3 the balance has gaps. */
        public double balanceErrorTc { get; set; }
        /*! dpRzEtc[mm], unit: "mm", description: Deep percolation from root zone for single calculation ET. This amount of water percolates from the root to the deep zone. */
        public double dpRzEtc { get; set; }
        /*! dpDzEtc[mm], unit: "mm", description: Deep percolation from deep zone for single calculation ET. This amount of water percolates from the deep zone to ground water. */
        public double dpDzEtc { get; set; }
        /*! drDiffEtc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public double drDiffEtc { get; set; }
        /*! autoIrrigationEtc[mm], unit: "mm", description: Sum of calculated irrigation demand for ET calculation. Fraction of wetted surface is not considered. */
        public double autoIrrigationEtc { get; set; }
        /*! autoNetIrrigationEtc[mm], unit: "mm", description: Sum of calculated irrigation demand for ET calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public double autoNetIrrigationEtc { get; set; }
        /*! interceptionAutoIrrEtc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public double interceptionAutoIrrEtc { get; set; }
        /*! balanceErrorEtc[mm], unit: "mm", description: Overall balance error for ET calculation between input water and output. This value must be near zero. The equation is: "precipitation + netIrrigation + drDiffEtc - interception - interceptionIrr - etAct - dpDzEtc + autoNetIrrigationEtc - interceptionAutoIrrEtc". If larger than 1E-3 the balance has gaps. */
        public double balanceErrorEtc { get; set; }
        /*! kcIni, unit: "none", description: Result from kcIni model, calculated crop coefficient for initial plant stage. */
        public double kcIni { get; set; }
        /*! ksMeanTcInitial, unit: "none", description: Mean value for water stress factor "Ks" in initial stage of plant growing for E plus T calculation. */
        public double ksMeanTcInitial { get; set; }
        /*! ksMeanTcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for E plus T calculation. */
        public double ksMeanTcDevelopment { get; set; }
        /*! ksMeanTcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for E plus T calculation. */
        public double ksMeanTcMid_season { get; set; }
        /*! ksMeanTcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for E plus T calculation. */
        public double ksMeanTcLate_season { get; set; }
        /*! ksMeanEtcInitial, unit: "none", description: Mean value for water stress factor "Ks" in initial stage of plant growing for ET calculation. */
        public double ksMeanEtcInitial { get; set; }
        /*! ksMeanEtcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for ET calculation. */
        public double ksMeanEtcDevelopment { get; set; }
        /*! ksMeanEtcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for ET calculation. */
        public double ksMeanEtcMid_season { get; set; }
        /*! ksMeanEtcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for ET calculation. */
        public double ksMeanEtcLate_season { get; set; }
        /*! yieldReductionTc, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public double yieldReductionTc { get; set; }
        /*! yieldReductionTcInitial, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public double yieldReductionTcInitial { get; set; }
        /*! yieldReductionTcDevelopment, unit: "none", description: Yield reduction factor due to water stress in development stage of plant growing for E plus T calculation. */
        public double yieldReductionTcDevelopment { get; set; }
        /*! yieldReductionTcMid_season, unit: "none", description: Yield reduction factor due to water stress in mid_season stage of plant growing for E plus T calculation. */
        public double yieldReductionTcMid_season { get; set; }
        /*! yieldReductionTcLate_season, unit: "none", description: Yield reduction factor due to water stress in late_season stage of plant growing for E plus T calculation. */
        public double yieldReductionTcLate_season { get; set; }
        /*! yieldReductionEtc, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for ET calculation. */
        public double yieldReductionEtc { get; set; }
        /*! yieldReductionEtcInitial, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for ET calculation. */
        public double yieldReductionEtcInitial { get; set; }
        /*! yieldReductionEtcDevelopment, unit: "none", description: Yield reduction factor due to water stress in development stage of plant growing for ET calculation. */
        public double yieldReductionEtcDevelopment { get; set; }
        /*! yieldReductionEtcMid_season, unit: "none", description: Yield reduction factor due to water stress in mid_season stage of plant growing for ET calculation. */
        public double yieldReductionEtcMid_season { get; set; }
        /*! yieldReductionEtcLate_season, unit: "none", description: Yield reduction factor due to water stress in late_season stage of plant growing for ET calculation. */
        public double yieldReductionEtcLate_season { get; set; }
        /*! dailyValues, description: Each line contains values for one day from "seedDate" to "harvestDate". This includes input values, intermediate results, evaporation, transpiration, evapotranspiration and soil water balance. */
        public IDictionary<DateTime, TranspirationResult> dailyValues { get; set; }
        /*! kcIniResult, description: Intermediate values and result of the "kcIni" model. */
        public KcIniResult kcIniResult { get; set; }
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
         * \param   climate     The climate.
         * \param   plant       The plant.
         * \param   soil        The soil.
         * \param   location    latitude, longitude and altitude
         * \param   irrigationSchedule  data with irrigation amounts per calculation step
         * \param   seedDate    The seed date.
         * \param   harvestDate The harvest date.
         * \param   initialConditions Soil water content at begin of calculation. Six depletion values for root zone, deep zone and evaporation layer for two calculation approaches are maintained.
         * \param   autoIrr             Set to true to automatically irrigate.
         */
        public static TranspirationResult TranspirationCalc(
            Climate climate,
            Plant plant,
            Soil soil,
            IrrigationSchedule irrigationSchedule,
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            DateTime start,
            DateTime end,
            SoilConditionsDual initialConditions,
            bool autoIrr

        )
        {
            return TranspirationCalc(climate, plant, soil, irrigationSchedule, location, seedDate, harvestDate, start, end, initialConditions, null, null, null, null, null, autoIrr, null, null, null, null, null, null, null);
        }


        /*!
         * \brief   Transpiration calculation.
         *
         * \param   climate     The climate.
         * \param   plant       The plant.
         * \param   soil        The soil.
         * \param   location    latitude, longitude and altitude
         * \param   irrigationSchedule  data with irrigation amounts per calculation step
         * \param   seedDate    The seed date.
         * \param   harvestDate The harvest date.
         * \param   initialConditions Soil water content at begin of calculation. Six depletion values for root zone, deep zone and evaporation layer for two calculation approaches are maintained.
         * \param   _as         Regression coefficient for calculation of global radiation. If omitted, FAO recommended default value of 0.25 is used.
         * \param   _bs         Regression coefficient for calculation of global radiation. If omitted, FAO recommended default value of 0.5 is used.
         * \param   ct          May be null, default: 17.8, Empirical temperature coefficient, if omitted, recommended default value of 17.8 (Hargreaves 1994) is used.
         * \param   ch          May be null, default: 0.0023, Empirical Hargreaves coefficient, if omitted, recommended default value of 0.0023 (Hargreaves 1994) is used.
         * \param   eh          May be null, default: 0.5, Empirical hargreaves exponent, if omitted, recommended default value of 0.5 (Hargreaves 1994) is used.
         * \param   autoIrr             Set to true to automatically irrigate.
         * \param   autoIrrLevel        If "autoIrr" is true, it will start at this fraction of available soil water. The value 0.8 is for 80%.
         * \param   autoIrrCutoff       If "autoIrr" is true _and_ "autoIrrAmount" is 0 (automatic amount calculation), then the amount is calculated to saturate the soil right up to "autoIrrCutoff" value.
         * \param   autoIrrAmount       If "autoIrr" is true, this amount of irrigation is added per day, if available soil water drops below "autoIrrLevel". If this value is 0, then the amount of drainage from last day is added if available soil water drops below "autoIrrLevel".
         * \param   autoIrrType         If "autoIrr" is true, this type of irrigation system is used for calculation of interception and fraction wetted.
         * \param   autoIrrStartDay     If "autoIrr" is used, irrigation is started at this day of plant development. No automatic irrigation is added before this day.
         * \param   autoIrrEndDay       If "autoIrr" is used, irrigation ends at this day of plant development. No automatic irrigation is added after this day.
         * \param   eFactor             The calculated value of evaporation is always multiplied by this factor to reduce evaporation because of e.g. mulching.
         *
         * \return  A TranspirationResult class of values.
         *   
         * \code
         * 'create a location for et0 calculation needed values latitude and altitude
         * 'and to get nearest climate station data from webservice
         * Dim location As atbApi.Location = New atbApi.Location(48.5, 9.3, 450)
         * 
         * 'create climate with daily timestep
         * Dim climate As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.day)
         * 
         * 'define interval to load climate data and load data asyncron from webservice
         * 'return value count is the number of datasets and should be 366
         * Dim climateStart As DateTime = New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc)
         * Dim climateEnd As DateTime = New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc)
         * Dim count As Integer = Await climate.loadFromATBWebService(location, climateStart, climateEnd)
         * 
         * 'as an alternative add climate data by yourself, at least max_temp and min_temp are required
         * 'Dim loopDate As DateTime = climateStart
         * 'While (loopDate <= climateEnd)
         * '    Dim values As atbApi.data.ClimateValues = New atbApi.data.ClimateValues()
         * '    values.max_temp = my_max_temp[loopDate]         'replace my_max_temp[loopDate] with your data
         * '    values.min_temp = my_min_temp[loopDate]         'replace my_min_temp[loopDate] with your data
         * '    climate.addValues(loopDate, values)
         * '    loopDate = loopDate.AddDays(1)
         * 'End While
         * 
         * 'optional: load altitude from webservice if unknown
         * 'Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(location)
         * 'location.alt = altitude
         * 
         * 'create plant from dll internal plant database
         * Dim plant As atbApi.data.Plant = New atbApi.data.Plant("ATB_maize_grain_arid_climate")
         * 
         * 'create soil from dll internal standard soils
         * Dim soil As atbApi.data.Soil = New atbApi.data.Soil("USDA-soilclass_clay_loam")
         * 
         * 'create new soil water conditions if sequential calculation starts
         * 'it is important, to keep transpirationResult.lastConditions and use this
         * 'instead for consecutive crops on the same field
         * Dim soilConditions As atbApi.data.SoilConditionsDual = New atbApi.data.SoilConditionsDual()
         * 
         * 'define seedDate and harvestDate
         * Dim seedDate As DateTime = New DateTime(2012, 4, 12, 0, 0, 0, DateTimeKind.Utc)
         * Dim harvestDate As DateTime = New DateTime(2012, 10, 5, 0, 0, 0, DateTimeKind.Utc)
         * 
         * 'finally start calculation
         * Dim transpirationResult As atbApi.TranspirationResult = atbApi.Transpiration.TranspirationCalc(climate, plant, soil, Nothing, location, seedDate, harvestDate, soilConditions, False)
         * 
         * 'keep this for next calculation on this field
         * Dim nextSoilConditions As atbApi.data.SoilConditionsDual = transpirationResult.lastConditions
         * \endcode
         */

        public static TranspirationResult TranspirationCalc(
            Climate climate,
            Plant plant,
            Soil soil,
            IrrigationSchedule irrigationSchedule,
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            DateTime start,
            DateTime end,
            SoilConditionsDual initialConditions,
            Double? _as,
            Double? _bs,
            Double? ct,
            Double? ch,
            Double? eh,
            bool autoIrr,
            Double? _autoIrrLevel,
            Double? _autoIrrCutoff,
            Double? _autoIrrAmount,
            IrrigationType autoIrrType,
            Int32? autoIrrStartDay,
            Int32? autoIrrEndDay,
            Double? _eFactor
        )
        {
            var profileStart = DateTime.Now;
            var result = new TranspirationResult();

            //plausibility checks
            if (start < seedDate) start = seedDate;
            if (end > harvestDate) end = harvestDate;
            if (seedDate >= harvestDate) return result;

            //fill variables
            var autoIrrLevel = _autoIrrLevel != null ? (double)_autoIrrLevel : 0.8;
            var autoIrrCutoff = _autoIrrCutoff != null ? (double)_autoIrrCutoff : 0.9;
            var autoIrrAmount = _autoIrrAmount != null ? (double)_autoIrrAmount : 2;
            var maxDepth= 1.999999999999;
            var interval= (Int32)(harvestDate - seedDate).TotalDays;
            var stageTotal= plant.stageTotal;
            if (DateTime.IsLeapYear(seedDate.Year) && seedDate.DayOfYear < 61) stageTotal += 1;
            if (DateTime.IsLeapYear(harvestDate.Year) && harvestDate.DayOfYear > 59) stageTotal += 1;

            for (DateTime loopDate = start; loopDate <= end; loopDate = loopDate.AddDays(1))
            {
                //var tcLoop= function(loopDate, stageTotal, lastConditions) {
                var plantDay = (Int32)Math.Round(((loopDate - seedDate).TotalDays) * (stageTotal / interval), 0);
                if (plantDay < 1) plantDay= 1;
                if (plantDay > stageTotal) plantDay= stageTotal;

                var climateSet = climate.getValues(loopDate);
                var plantSet = plant.getValues(plantDay);
                var soilSet = soil.getValues(Math.Min(initialConditions.et.zr, maxDepth));
                var soilSetMax = soil.getValues(maxDepth);
                var et0 = Et0.Et0Calc(climate, loopDate, location, _as, _bs, ct, ch, eh);

                var irrigation= 0.0;
                var netIrrigation= 0.0;
                var irrigationType = irrigationSchedule.irrigationType;
                var irrigationFw= irrigationType.fw;
                result.irrigationFw= irrigationFw;
                var autoIrrigationTc= 0.0;
                var autoNetIrrigationTc= 0.0;
                var autoIrrigationEtc= 0.0;
                var autoNetIrrigationEtc= 0.0;
                var autoIrrigationType= autoIrrType;
                var autoIrrigationFw= autoIrrType.fw;
                result.autoIrrigationFw = autoIrrigationFw;
                var netPrecipitation= 0.0;

                //common calculations for Tc and ETc
                var zr= Math.Min(maxDepth, (double)plantSet.Zr);
                //taw in root zone
                var tawRz= 1000 * (soilSet.Qfc - soilSet.Qwp) * zr;
                var tawMax= 1000 * (soilSetMax.Qfc - soilSetMax.Qwp) * maxDepth;
                //taw in deep zone
                var tawDz= tawMax - tawRz;
                var ze= soilSet.Ze != null ? soilSet.Ze : 0.1;
                var tew= 1000 * (soilSet.Qfc - 0.5 * soilSet.Qwp) * ze;
                var cf= (1 - Math.Exp(-(double)plantSet.LAI * 0.385));

                if (irrigationSchedule.schedule.ContainsKey(loopDate)) {
                    irrigationSchedule.schedule.TryGetValue(loopDate, out irrigation);
                    netIrrigation= irrigation * irrigationFw;
                    /*
                    loopResult.irrigation= irrigation;
                    loopResult.irrigationFw= irrigationFw;
                    loopResult.netIrrigation= netIrrigation;
                    loopResult.indirectWater= indirectWater;
                    */
                }

                //calculate auto irrigation
                var soilSaturationTc= tawRz != 0 ? (tawRz - initialConditions.e.drRz) / tawRz : 0;
                var soilSaturationEtc= tawRz != 0 ? (tawRz - initialConditions.et.drRz) / tawRz : 0;
                var autoIrrWindow= true;
                if ((autoIrrStartDay != null && plantDay < autoIrrStartDay) || (autoIrrEndDay != null && plantDay > autoIrrEndDay)) autoIrrWindow= false;

                if (autoIrr && autoIrrWindow && !(plantSet.isFallow == true) && tawRz != 0) {
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
/*
                //calculate interception for irrigation for auto and data based irrigation
                if (data.plantSet.LAI === 0 || a === 0) {
                    var interception= 0
                } else {
                    var interception= a * data.plantSet.LAI * (1 - 1 / (1 + (data.plantSet.CF * data.climateSet.precipitation) / (a * data.plantSet.LAI)));
                }
                var interceptionIrr= a * data.plantSet.LAI * (1 - 1 / (1 + (data.plantSet.CF * netIrrigation) / (a * data.plantSet.LAI)));
                var interceptionAutoIrrTc= a * data.plantSet.LAI * (1 - 1 / (1 + (data.plantSet.CF * autoNetIrrigationTc) / (a * data.plantSet.LAI)));
                var interceptionAutoIrrEtc= a * data.plantSet.LAI * (1 - 1 / (1 + (data.plantSet.CF * autoNetIrrigationEtc) / (a * data.plantSet.LAI)));
                if (irrigationTypeData) {
                    interceptionIrr= interceptionIrr * irrigationTypeData.interception;
                }
                else {
                    interceptionIrr= 0;
                }
                if (autoIrrigationTypeData) {
                    interceptionAutoIrrTc= interceptionAutoIrrTc * autoIrrigationTypeData.interception;
                    interceptionAutoIrrEtc= interceptionAutoIrrEtc * autoIrrigationTypeData.interception;
                }
                else {
                    interceptionAutoIrrTc= 0;
                    interceptionAutoIrrEtc= 0;
                }
*/
/*
                    var eDebug;
                    var eCalculation= function(lastConditions) {
                        var result= {};
                        var rewFactor= 2.2926;
                        var kcMax= 1.2;
                        var kcMin= 0;
                        var kcb= 0;
                        var fc= 0;
                        if (!data.plantSet.isFallow) {
                            kcb= data.plantSet.Kcb;
                            kcMax= 1.2 + (0.04 * (data.climateSet.windspeed - 2) - 0.004 * (data.climateSet.humidity - 45)) * Math.pow((data.plantSet.height / 3), 0.3);
                            kcMax= Math.max(kcMax, kcb + 0.05);
                            kcMax= Math.min(kcMax, 1.3);
                            kcMax= Math.max(kcMax, 1.05);
                            kcMin= 0.175;
                            fc= Math.pow(Math.max((kcb - kcMin), 0.01) / (kcMax - kcMin), 1 + 0.5 * data.plantSet.height);
                            fc= Math.min(0.99, fc);
                        }
                        result.kcMin= kcMin;
                        result.kcMax= kcMax;
                        result.kcb= kcb;
                        result.fc= fc;

                        var few= Math.min(1 - fc, Math.min(irrigationFw, autoIrrigationFw));
                        if ((netIrrigation || autoNetIrrigationTc) &&
                            ((irrigationTypeData && irrigationTypeData.type === "drip") || (autoIrrigationTypeData && autoIrrigationTypeData.type === "drip"))) {
                            few= Math.min(1 - fc, (1 - (2 / 3) * fc) * Math.min(irrigationFw, autoIrrigationFw));
                        }

                        result.few= few;
                        result.tew= tew;
                        var rew= tew / rewFactor;
                        result.rew= rew;
                        var kr= 1;
                        if (lastConditions.de > rew) {
                            kr= (tew - lastConditions.de) / (tew - rew);
                            kr= Math.max(0, kr);
                        }
                        if (rew === 0) kr= 0;
                        result.kr= kr;
                        var ke= Math.min(kr * (kcMax - kcb), few * kcMax);
                        result.ke= ke;
                        var e= ke * data.et0.et0;
                        e= e * eFactor;
                        result.e= e;
                        var netPrecititationTc= data.climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigationTc - interceptionAutoIrrTc;
                        var dpe= netPrecititationTc - lastConditions.de;
                        dpe= Math.max(0, dpe);
                        var de= lastConditions.de - netPrecititationTc + e / few + dpe;
                        de= Math.max(0, de);
                        de= Math.min(tew, de);
                        lastConditions.de= de;
                        lastConditions.dpe= dpe;
                        result.de= de;
                        result.dpe= dpe;
                        return result;
                    }

                    var tcCalculation= function(kc, lastConditions, resultSuffix) {
                        if (resultSuffix === "Tc") {
                            var interceptionAutoIrr= interceptionAutoIrrTc;
                            var autoIrrigation= autoIrrigationTc;
                            var autoNetIrrigation= autoNetIrrigationTc;
                        }
                        else {
                            var interceptionAutoIrr= interceptionAutoIrrEtc;
                            var autoIrrigation= autoIrrigationEtc;
                            var autoNetIrrigation= autoNetIrrigationEtc;
                        }
                        //recalculate netPrecipitation for Etc/Tc

                        netPrecipitation= Math.max(0, data.climateSet.precipitation - interception + netIrrigation - interceptionIrr + autoNetIrrigation - interceptionAutoIrr);
                        //Tc / Kcb calculations
                        var kcAdj= kc;
                        if ((data.plantSet.name === "mid_season" || data.plantSet.name === "late_season") && kc > 0.45) {
                            kcAdj= kc + (0.04 * (data.climateSet.windspeed - 2) - 0.004 * (data.climateSet.humidity - 45)) * Math.pow((data.plantSet.height / 3), 0.3);
                        }
                        var etc= data.et0.et0 * kcAdj;
                        var pAdj= data.plantSet.p + 0.04 * (5 - etc);
                        //0.1 < pAdj < 0.8
                        pAdj= Math.min(0.8, pAdj);
                        pAdj= Math.max(0.1, pAdj);
                        var raw= tawRz * pAdj;
                        var ks= raw !== 0 ? 1 : 0;
                        if ((lastConditions.drRz > raw && raw !== 0) && !data.plantSet.isFallow) {
                            ks= Math.max(0, (tawRz - lastConditions.drRz) / (tawRz - raw));
                        }
                        var etAct= etc * ks;
                        var etSum= etAct;
                        if (resultSuffix === "Tc") {
                            var e= eCalculation(lastConditions);
                            eDebug= e;
                            loopResult.e= e.e;
                            loopResult.eAct= e.e;
                            loopResult.de= e.de;
                            loopResult.dpe= e.dpe;
                            loopResult.few= e.few;
                            loopResult.tew= e.tew;
                            loopResult.rew= e.rew;
                            loopResult.kr= e.kr;
                            loopResult.ke= e.ke;
                            loopResult.kcMax= e.kcMax;
                            loopResult.kcMin= e.kcMin;
                            etSum += e.e;
                        }

                        //moved border rootZone -> adjust drainage
                        if (use zr!!!data.plantSet.Zr != lastConditions.zr) {
                            var dzDepth= maxDepth -  lastConditions.zr;
                            var zDiff= use zr!!!data.plantSet.Zr - lastConditions.zr;
                            var drDiff= zDiff * lastConditions.drDz / dzDepth;
                            lastConditions.drRz += drDiff;
                            lastConditions.drDz -= drDiff;
                        }

                        //calculate soil water balance
                        var dpRz= Math.max(0, netPrecipitation - etSum - lastConditions.drRz);
                        var drRz= lastConditions.drRz - netPrecipitation + etSum + dpRz;

                        if (drRz < 0) {
                            //negative drainage should not happen -> percolate excess water to deep zone
                            dpRz -= drRz;
                            drRz= 0;
                        }
                        else if (drRz > tawRz) {
                            //drainage exceeds taw -> adjust this day values to stay beyond this limit
                            var drRzExceeded= drRz - tawRz;
                            loopResult['drRzExceeded' + resultSuffix]= drRzExceeded;

                           if (resultSuffix === "Tc") {
                                var eFactor= e.e / etSum;
                                var etFactor= etAct / etSum;
                                etAct= Math.min(0, etAct - drRzExceeded * etFactor);
                                loopResult.eAct= Math.min(0, e.e - drRzExceeded * eFactor);
                                etSum= loopResult.eAct + etAct;
                            }
                            else {
                                etAct= Math.min(0, etAct - drRzExceeded);
                                etSum= etAct;
                            }

                            drRz= lastConditions.drRz - netPrecipitation + etSum + dpRz;
                        }

                        var dpDz= Math.max(0, dpRz - lastConditions.drDz);
                        var drDz= lastConditions.drDz - dpRz + dpDz;

                        if (drDz < 0) {
                            //negative drainage should not happen -> deep percolate excess water
                            dpDz -= drDz;
                            drDz= 0;
                        }
                        if (drDz > tawDz) {
                            //drainage exceeds taw should not happen in deep zone -> write exceed value to result table
                            loopResult['drDzExceeded' + resultSuffix]= drDz - tawDz;
                            drDz= tawDz;
                        }

                        if ((data.climateSet.precipitation + netIrrigation + autoNetIrrigation) > 0) {
                            loopResult["precIrrEff" + resultSuffix]= (netPrecipitation - dpDz) / (data.climateSet.precipitation + netIrrigation + autoNetIrrigation);

                            //calculate soil water storage efficiency
                            if (lastConditions.drRz.toFixed(5) === "0.00000") {
                                loopResult["soilStorageEff" + resultSuffix]= 0;
                            }
                            else {
                                loopResult["soilStorageEff" + resultSuffix]= (netPrecipitation - dpRz) / lastConditions.drRz;
                            }
                        }
                        loopResult["drDiff" + resultSuffix]= (drRz + drDz) - (lastConditions.drRz + lastConditions.drDz);


                        lastConditions.drRz= drRz;
                        lastConditions.tawRz= tawRz;
                        lastConditions.drDz= drDz;
                        lastConditions.tawDz= tawDz;
                        lastConditions.zr= use zr!!!data.plantSet.Zr;

                        loopResult.tawRz= tawRz;
                        loopResult.tawDz= tawDz;


                        if (resultSuffix === "Etc") {
                            loopResult.kc= kc;
                            loopResult.kcAdj= kcAdj;
                            loopResult.etc= etc;
                            loopResult.etAct= etAct;
                        } else {
                            loopResult.kcb= kc;
                            loopResult.kcbAdj= kcAdj;
                            loopResult.tc= etc;
                            loopResult.tAct= etAct;
                        }
                        loopResult["pAdj" + resultSuffix]= pAdj;
                        loopResult["raw" + resultSuffix]= raw;
                        loopResult["ks" + resultSuffix]= ks;
                        loopResult["dpRz" + resultSuffix]= dpRz;
                        loopResult["drRz" + resultSuffix]= drRz;
                        loopResult["dpDz" + resultSuffix]= dpDz;
                        loopResult["drDz" + resultSuffix]= drDz;
                        loopResult["autoIrrigation" + resultSuffix]= autoIrrigation;
                        loopResult["autoNetIrrigation" + resultSuffix]= autoNetIrrigation;
                        loopResult["interceptionAutoIrr" + resultSuffix]= interceptionAutoIrr;
                        loopResult["netPrecipitation" + resultSuffix]= netPrecipitation;
                        buildKsMean(resultSuffix, data.plantSet.name, ks, data.plantSet.Ky);
                        if (data.plantSet.Ky) {
                            loopResult["yieldReduction" + resultSuffix]= data.plantSet.Ky * (1 - ks);
                            loopResult.plantKy= data.plantSet.Ky;
                        }
                    };

                    var getKcIni= function(cb) {
                        if ((data.plantSet.name !== "initial") && (data.plantSet.name !== "development")) {
                            return cb(null, data.plantSet.Kc);
                        }
                        if (data.plantSet.start === undefined) {
                            data.plantSet.start= {day: 1};
                        }
                        if (data.plantSet.end === undefined) {
                            data.plantSet.end= {day: 366};
                        }
                        if (data.plantSet.name === "development") {
                            //if kcIni undefined -> plant without initial phase -> fix Kc for development stage
                            if (kcIni === undefined) {
                                kcIni= data.plantSet.Kc;
                            }
                            var kcDev= (plantDay - data.plantSet.start.day) * (data.plantSet.Kc - kcIni) / (data.plantSet.end.day - data.plantSet.start.day + 1) + kcIni;
                            return cb(null, kcDev);
                        }
                        if (kcIni !== undefined) {
                            return cb(null, kcIni);
                        }
                        var iniLength= Math.round((data.plantSet.end.day - data.plantSet.start.day + 1) / (stageTotal / interval));

                        var kcIniArgs= {
                            location: location,
                            polygonKey: polygonKey,
                            startDate: new Date(loopDate),
                            initialLength: iniLength,
                            et0: et0Lib,
                            soil: soilLib,
                            climate: climateLib,
                            management: managementLib,
                            //geaendert am 15.9.2014
                            //ze: ze,
                            ze: use zr!!!data.plantSet.Zr,
                            depletionDeInitial: args.depletionDeInitial !== undefined ? args.depletionDeInitial : args.depletionRzInitial !== undefined ? args.depletionRzInitial : 0.1,
                            autoIrr: data.plantSet.isFallow ? false : autoIrr,
                            autoIrrLevel: autoIrrLevel,
                            autoIrrAmount: autoIrrAmount,
                            autoIrrCutoff: autoIrrCutoff,
                            autoIrrType: autoIrrType,
                            autoIrrStartDay: autoIrrStartDay,
                            autoIrrEndDay: autoIrrEndDay,
                            eFactor: eFactor,
                        };

                        return lib.getInputSet("kcIni", kcIniArgs, function(err, kcIniResult) {
                            if (err) return cb(err);
                            result.tables.kcIni= kcIniResult;
                            kcIni= kcIniResult.kcIni;
                            result.kcIni= kcIniResult.kcIni;
                            return cb(null, kcIni);
                        });
                    };

                    var finishLoop= function() {
                        resultTable.push(loopResult);
                        for (var field in  resultFieldUnits) {
                            switch(field) {
                            case "et0":
                                result[field] += data.et0.et0;
                                break;
                            case "precipitation":
                                result[field] += data.climateSet.precipitation;
                                break;
                            case "irrigation":
                                result[field] += irrigation;
                                break;
                            case "netIrrigation":
                                result[field] += netIrrigation;
                                break;
                            case "interception":
                                result[field] += interception;
                                break;
                            default:
                                if (!(field in resultBalanceFields)) result[field] += loopResult[field];
                            }
                        }

                        if (isDebug) {
                            debugValues.push({
                                loopDate: loopDate,
                                plantDay: plantDay,
                                plantZr: use zr!!!data.plantSet.Zr,
                                et0: data.et0.et0,
                                precipitation: data.climateSet.precipitation,
                                irrigation: irrigation,
                                netIrrigation: netIrrigation,
                                interception: interception,
                                interceptionIrr: interceptionIrr,
                                netPrecipitation: netPrecipitation,
                                tawRz: tawRz,
                                tawDz: tawDz,
                                tawMax: tawMax,
                                kcb: loopResult.kcb,
                                kcbAdj: loopResult.kcbAdj,
                                tc: loopResult.tc,
                                pAdjTc: loopResult.pAdjTc,
                                rawTc: loopResult.rawTc,
                                ksTc: loopResult.ksTc,
                                tAct: loopResult.tAct,
                                dpTc: loopResult.dpTc,
                                drTc: loopResult.drTc,
                                autoIrrigationTc: autoIrrigationTc,
                                interceptionAutoIrrTc: interceptionAutoIrrTc,
                                ksMeanTc: ksMeanTc,
                                kc: loopResult.kc,
                                kcAdj: loopResult.kcAdj,
                                etc: loopResult.etc,
                                pAdjEtc: loopResult.pAdjEtc,
                                rawEtc: loopResult.rawEtc,
                                ksEtc: loopResult.ksEtc,
                                etAct: loopResult.etAct,
                                dpEtc: loopResult.dpEtc,
                                drEtc: loopResult.drEtc,
                                autoIrrigationEtc: autoIrrigationEtc,
                                interceptionAutoIrrEtc: interceptionAutoIrrEtc,
                                autoIrrigationType: autoIrrigationType,
                                ksMeanEtc: ksMeanEtc,
                                e: eDebug,
                                lastConditions: lastConditions,
                                valueCollectorData: data,
                            });
                        }

                        var nextDate= new Date(loopDate);
                        nextDate.setUTCDate(loopDate.getUTCDate() + 1);
                        //recursion stop condition
                        if (nextDate > harvestDate) {
                            result.lastConditions= lib.clone(lastConditions);
                            delete result.lastConditions.__noLib;
                            buildBalance(result, lastConditions);
                            buildYieldReduction();
                            for (var field in  resultFieldUnits) {
                                result[lib.addUnit(field, resultFieldUnits[field])]= result[field];
                                delete result[field];
                            }
                            for (var field in  resultBalanceFields) {
                                result[lib.addUnit(field, resultBalanceFields[field])]= result[field];
                                delete result[field];
                            }
                            debug(debugValues);
                            result.runtimeMs= +new Date() - +profileStart;
                            return callback(null, result);
                        } else {
                            return tcLoop(nextDate, stageTotal, lib.clone(lastConditions));
                        }
                    };

                    loopResult.loopDate= loopDate;
                    loopResult.plantDay= plantDay;
                    loopResult.plantZr= use zr!!!data.plantSet.Zr;
                    loopResult.plantStage= data.plantSet.name;
                    loopResult.et0= data.et0.et0;
                    loopResult.precipitation= data.climateSet.precipitation;
                    loopResult.interception= interception;
                    loopResult.interceptionIrr= interceptionIrr;
                    loopResult.tawRz= tawRz;
                    loopResult.tawDz= tawDz;

                    tcCalculation(data.plantSet.Kcb, lastConditions.e, "Tc");
                    return getKcIni(function(err, kcIniValue) {
                        if (err) return errorExit(err);
                        tcCalculation(kcIniValue, lastConditions.et, "Etc");
                        return finishLoop();
                    });
                });
            });
        });
    }; //tcLoop

    // query basedata and put it into result
    var collectorObject= {
        plantBaseData: function(cb) {
            return plantLib.getBaseData(args, cb);
        },
        soilBaseData: function(cb) {
            return soilLib.getBaseData(args, cb);
        },
        climateBaseData: function(cb) {
            return climateLib.getBaseData(args, cb);
        },
        plantSet: function(cb) {
            return plantLib.getSet({ day: 1 }, cb);
        },
        et0Lib: function(cb) {
            return lib.getInputLib("et0", cb);
        },
    };
    if (managementLib) {
        collectorObject.managementBaseData= function(cb) {
            return managementLib.getBaseData(args, cb);
        };
    }


    return lib.valueCollector(collectorObject, function(err, data) {
        if (err) return errorExit(err);
        if (!data.plantBaseData) return errorExit("ERROR: Unable to get \"plantBaseData\" from database, cannot continue! args: " + lib.inspect(args));
        if (!data.soilBaseData) return errorExit("ERROR: Unable to get \"soilBaseData\" from database, cannot continue! args: " + lib.inspect(args));
        if (!data.climateBaseData) return errorExit("ERROR: Unable to get \"climateBaseData\" from database, cannot continue! args: " + lib.inspect(args));
        if (managementLib && !data.managementBaseData) return errorExit("ERROR: Unable to get \"managementBaseData\" from database, cannot continue! args: " + lib.inspect(args));
        if (!data.et0Lib) return errorExit("ERROR: Unable to get lib for \"et0\" input, cannot continue! " + "args: " + lib.inspect(args));

        var plantIdMsg= " dataObjId: " + data.plantBaseData._id + ", plantDay: 1";
        if (!data.plantSet) return errorExit("ERROR: Unable to get \"plantSet\" from database, cannot continue!" + plantIdMsg);
        if (data.plantSet.Zr === undefined) return errorExit("ERROR: Unable to get \"plant->Zr\" from database, cannot continue!" + plantIdMsg);
        //adjust Zr to maxDepth
        data.plantSet.Zr= Math.min(maxDepth, data.plantSet.Zr);

        et0Lib= data.et0Lib;

        //save objectIds in result
        result.plant= data.plantBaseData.name;
        result.soil= data.soilBaseData.name;
        result.climate= [ data.climateBaseData.name ];
        result.seedDate= seedDate;
        result.harvestDate= harvestDate;
        result.inputBaseData= {
            plant: data.plantBaseData,
            soil: data.soilBaseData,
            climate: data.climateBaseData,
        };
        if (managementLib) {
            result.management= data.managementBaseData.name;
            result.inputBaseData.management= data.managementBaseData;
        }
        if (isDebug) {
            debugValues.push({
                interval: interval,
                valueCollectorData: data,
            });
        }


        if (!location) location= data.climateBaseData.location;
        result.location= location;

        if (args.lastConditions) initialConditions= args.lastConditions;

        return lib.valueCollector({
            soilSet: function(cb) {
                return soilLib.getSet({ z: Math.min(data.plantSet.Zr, maxDepth), location: location }, cb);
            },
            soilSetMax: function(cb) {
                return soilLib.getSet({ z: maxDepth, location: location }, cb);
            },
        }, function(err, soilData) {
            if (err) return errorExit(err);

            var soilIdMsg= " dataObjId: " + result.inputBaseData.soil._id + " name: " + result.inputBaseData.soil.name + ", z: " + data.plantSet.Zr + ", location: " + location;
            var soilMaxIdMsg= " dataObjId: " + result.inputBaseData.soil._id + " name: " + result.inputBaseData.soil.name + ", z: " + maxDepth + ", location: " + location;
            if (!soilData.soilSet) return errorExit("ERROR: Unable to get \"soilSet\" from database, cannot continue!" + soilIdMsg);
            if (soilData.soilSet.Qwp === undefined) return errorExit("ERROR: Unable to get \"soil->Qwp\" from database, cannot continue!" + soilIdMsg);
            if (soilData.soilSet.Qfc === undefined) return errorExit("ERROR: Unable to get \"soil->Qfc\" from database, cannot continue!" + soilIdMsg);
            if (!soilData.soilSetMax) return errorExit("ERROR: Unable to get \"soilSetMax\" from database, cannot continue!" + soilMaxIdMsg);
            if (soilData.soilSetMax.Qwp === undefined) return errorExit("ERROR: Unable to get \"soil->Qwp\" from database, cannot continue!" + soilMaxIdMsg);
            if (soilData.soilSetMax.Qfc === undefined) return errorExit("ERROR: Unable to get \"soil->Qfc\" from database, cannot continue!" + soilMaxIdMsg);

            var tawRz= 1000 * (soilData.soilSet.Qfc - soilData.soilSet.Qwp) * data.plantSet.Zr;
            tawMax= 1000 * (soilData.soilSetMax.Qfc - soilData.soilSetMax.Qwp) * maxDepth;
            var tawDz= tawMax - tawRz;
            var ze= soilData.soilSet.Ze ? soilData.soilSet.Ze : 0.1;

            //no initial conditions -> build from args and soil/plant parameters from day 1
            if (!initialConditions) {
                //start with initial rooting depth and weighted balance
                var zrInitial= args.rootZoneInitial !== undefined ? args.rootZoneInitial : data.plantSet.Zr;
                zrInitial= Math.max(0, Math.min(maxDepth, zrInitial));
                var drRzInitial= args.depletionRzInitial !== undefined ? args.depletionRzInitial * tawRz : 0.1 * tawRz;
                var drDzInitial= args.depletionDzInitial !== undefined ? args.depletionDzInitial * tawDz : 0.1 * tawDz;
                var drDeInitial= args.depletionDeInitial !== undefined ? args.depletionDeInitial * tawRz * (ze / zrInitial) : 0.1 * tawRz * (ze / zrInitial);
                initialConditions= {
                    e: {
                        drRz: drRzInitial,
                        tawRz: tawRz,
                        drDz: drDzInitial,
                        tawDz: tawDz,
                        de: drDeInitial,
                        dpe: 0,
                        zr: zrInitial,
                    },
                    et: {
                        drRz: drRzInitial,
                        tawRz: tawRz,
                        drDz: drDzInitial,
                        tawDz: tawDz,
                        zr: zrInitial,
                    },
                    __noLib: null,
                };
            }

            //adjust conditions only if border between root and deep zone moved
            if (data.plantSet.Zr !== initialConditions.e.zr) {
                //adjust drainage according to changed root depth
                var drSumE= initialConditions.e.drRz + initialConditions.e.drDz;
                var drSumEt= initialConditions.et.drRz + initialConditions.et.drDz;

                //catch special case root zone from max to 0
                if (initialConditions.e.zr === maxDepth && data.plantSet.Zr === 0) {
                    var drRzE= 0;
                    var drRzEt= 0;
                    var drDzE= initialConditions.e.drRz;
                    var drDzEt= initialConditions.et.drRz;
                }
                //catch special case root zone from 0 to max
                else if (initialConditions.e.zr === 0 && data.plantSet.Zr === maxDepth) {
                    var drRzE= initialConditions.e.drDz;
                    var drRzEt= initialConditions.et.drDz;
                    var drDzE= 0;
                    var drDzEt= 0;
                }
                //root zone shrinks -> add root drainage to deep zone
                else if (data.plantSet.Zr < initialConditions.e.zr) {
                    var tawRzRatio= (initialConditions.e.tawRz / initialConditions.e.zr) / (tawRz / data.plantSet.Zr);
                    if (isNaN(tawRzRatio)) tawRzRatio= 1;
                    var zrFactor= data.plantSet.Zr / initialConditions.e.zr;
                    var drRzE= initialConditions.e.drRz * zrFactor / tawRzRatio;
                    var drRzEt= initialConditions.et.drRz * zrFactor / tawRzRatio;
                    var drDzE= drSumE - drRzE;
                    var drDzEt= drSumEt - drRzEt;
                }
                //root zone grows -> add deep drainage to root zone
                else {
                    var tawDzRatio= (initialConditions.e.tawDz / (maxDepth - initialConditions.e.zr)) / (tawDz / (maxDepth - data.plantSet.Zr));
                    if (isNaN(tawDzRatio)) tawDzRatio= 1;
                    var zrFactor= (maxDepth - data.plantSet.Zr) / (maxDepth - initialConditions.e.zr);
                    var drDzE= initialConditions.e.drDz * zrFactor / tawDzRatio;
                    var drDzEt= initialConditions.et.drDz * zrFactor / tawDzRatio;
                    var drRzE= drSumE - drDzE;
                    var drRzEt= drSumEt - drDzEt;
                }

                initialConditions= {
                    e: {
                        drRz: drRzE,
                        tawRz: tawRz,
                        drDz: drDzE,
                        tawDz: tawDz,
                        de: initialConditions.e.de,
                        dpe: initialConditions.e.dpe,
                        zr: data.plantSet.Zr,
                    },
                    et: {
                        drRz: drRzEt,
                        tawRz: tawRz,
                        drDz: drDzEt,
                        tawDz: tawDz,
                        zr: data.plantSet.Zr,
                    },
                    __noLib: null,
                };
            }
            result.drDiffTc= -(initialConditions.e.drRz + initialConditions.e.drDz);
            result.drDiffEtc= -(initialConditions.et.drRz + initialConditions.et.drDz);
            result.initialConditions= lib.clone(initialConditions);
            delete result.initialConditions.__noLib;

            return tcLoop(startDate, stageTotal, initialConditions);
        });
    });

            */





//                var et0 = Et0.Et0Calc(climate, date, location, _as, _bs, ct, ch, eh);
                //if (et0 == null) continue;
                //result.et0 += et0.et0;
            }

            result.tAct = result.et0 * 1.2345678;
            result.lastConditions = initialConditions;
            result.runtimeMs = ((double)DateTime.Now.Ticks - (double)profileStart.Ticks) / 10000.0;

            return result;
        }
    }
}
