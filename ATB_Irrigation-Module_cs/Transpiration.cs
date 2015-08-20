/*!
 * \file    Transpiration.cs
 *
 * \brief   The irrigation module implementation.
 *
 * \author  Hunstock
 * \date    05.08.2015
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
        public static TranspirationResult TranspirationCalc(
            Climate climate,
            Plant plant,
            Soil soil,
            IrrigationSchedule irrigation,
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            SoilConditionsDual initialConditions,
            bool autoIrr

        )
        {
            return TranspirationCalc(climate, plant, soil, location, seedDate, harvestDate, initialConditions, null, null, null, null, null, autoIrr, null, null, null, null, null, null, null);
        }


        /*!
         * \brief   Transpiration calculation.
         *
         * \param   climate     The climate.
         * \param   plant       The plant.
         * \param   soil        The soil.
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
            Location location,
            DateTime seedDate,
            DateTime harvestDate,
            SoilConditionsDual initialConditions,
            Double? _as,
            Double? _bs,
            Double? ct,
            Double? ch,
            Double? eh,
            bool autoIrr,
            Double? autoIrrLevel,
            Double? autoIrrCutoff,
            Double? autoIrrAmount,
            IrrigationType autoIrrType,
            Int32? autoIrrStartDay,
            Int32? autoIrrEndDay,
            Double? eFactor
        )
        {
            var result = new TranspirationResult();
            var profileStart = DateTime.Now;
            for (DateTime date = seedDate; date <= harvestDate; date = date.AddDays(1))
            {
                var et0 = Et0.Et0Calc(climate, date, location, _as, _bs, ct, ch, eh);
                if (et0 == null) continue;
                result.et0 += et0.et0;
            }

            result.tAct = result.et0 * 1.2345678;
            result.lastConditions = initialConditions;
            result.runtimeMs = ((double)DateTime.Now.Ticks - (double)profileStart.Ticks) / 10000.0;

            return result;
        }
    }
}
