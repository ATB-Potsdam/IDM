/*!
 * An irrigation module.
 *
 * \author  Hunstock
 * \date    05.08.2015
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using atbApi;
using atbApi.data;
using local;

/*! 
 * \brief   namespace for all export classes
 * 
 */
namespace atbApi
{

    public class TranspirationResult
    {
        /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
        public Double? runtimeMs { get; set; }
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
        public Double? et0 { get; set; }
        /*! e[mm], unit: "mm", description: Sum of Evaporation for calculation period. */
        public Double? e { get; set; }
        /*! eAct[mm], unit: "mm", description: Sum of actual Evaporation for calculation period. */
        public Double? eAct { get; set; }
        /*! tc[mm], unit: "mm", description: Sum of Transpiration for calculation period. */
        public Double? tc { get; set; }
        /*! tAct[mm], unit: "mm", description: Sum of actual Transpiration for calculation period. */
        public Double? tAct { get; set; }
        /*! eAct+tAct[mm], unit: "mm", description: Sum of actual Evaporation and actual Transpiration for calculation period. */
        public Double? eActPlusTact { get; set; }
        /*! etc[mm], unit: "mm", description: Sum of Evapotranspiration for calculation period. */
        public Double? etc { get; set; }
        /*! etAct[mm], unit: "mm", description: Sum of actual Evapotranspiration for calculation period. */
        public Double? etAct { get; set; }
        /*! eAct+tAct-etAct[mm], unit: "mm", description: Balance error between the two calculation approaches E plus T and ET. If positive, "etAct" is smaller than "eAct" plus "tAct". */
        public Double? eActPlusTactMinusEtAct { get; set; }
        /*! precipitation[mm], unit: "mm", description: Precipitation sum for calculation period. */
        public Double? precipitation { get; set; }
        /*! interception[mm], unit: "mm", description: Interception sum for calculation period. This amount of water from "precipitation" is intercepted by leafes and does not reach the soil surface. */
        public Double? interception { get; set; }
        /*! irrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public Double? irrigationFw { get; set; }
        /*! irrigation[mm], unit: "mm", description: Irrigation sum for calculation period. */
        public Double? irrigation { get; set; }
        /*! netIrrigation[mm], unit: "mm", description: Netto irrigation sum for calculation period. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public Double? netIrrigation { get; set; }
        /*! interceptionIrr[mm], unit: "mm", description: Additional interception for the irrigated water. */
        public Double? interceptionIrr { get; set; }
        /*! dpRzTc[mm], unit: "mm", description: Deep percolation from root zone for dual calculation E plus T. This amount of water percolates from the root to the deep zone. */
        public Double? dpRzTc { get; set; }
        /*! dpDzTc[mm], unit: "mm", description: Deep percolation from deep zone for dual calculation E plus T. This amount of water percolates from the deep zone to ground water. */
        public Double? dpDzTc { get; set; }
        /*! drDiffTc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public Double? drDiffTc { get; set; }
        /*! autoIrrigationFw, unit: "none", description: Fraction of wetted surface, depending on irrigation method this value is usually between 0.3 for drip irrigation and 1 for sprinkler. */
        public Double? autoIrrigationFw { get; set; }
        /*! autoIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Fraction of wetted surface is not considered. */
        public Double? autoIrrigationTc { get; set; }
        /*! autoNetIrrigationTc[mm], unit: "mm", description: Sum of calculated irrigation demand for E plus T calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public Double? autoNetIrrigationTc { get; set; }
        /*! interceptionAutoIrrTc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public Double? interceptionAutoIrrTc { get; set; }
        /*! balanceErrorTc[mm], unit: "mm", description: Overall balance error for E plus T calculation between input water and output. The equation is: "precipitation + netIrrigation + drDiffTc - interception - interceptionIrr - e - tAct - dpDzTc + autoNetIrrigationTc - interceptionAutoIrrTc". This value must be near zero. If larger than 1E-3 the balance has gaps. */
        public Double? balanceErrorTc { get; set; }
        /*! dpRzEtc[mm], unit: "mm", description: Deep percolation from root zone for single calculation ET. This amount of water percolates from the root to the deep zone. */
        public Double? dpRzEtc { get; set; }
        /*! dpDzEtc[mm], unit: "mm", description: Deep percolation from deep zone for single calculation ET. This amount of water percolates from the deep zone to ground water. */
        public Double? dpDzEtc { get; set; }
        /*! drDiffEtc[mm], unit: "mm", description: Soil drainage difference between "initialConditions" and "lastConditions". If positive, the soil is more drained and this amount of water was additional available for the plant. If negative, the soil is more saturated. */
        public Double? drDiffEtc { get; set; }
        /*! autoIrrigationEtc[mm], unit: "mm", description: Sum of calculated irrigation demand for ET calculation. Fraction of wetted surface is not considered. */
        public Double? autoIrrigationEtc { get; set; }
        /*! autoNetIrrigationEtc[mm], unit: "mm", description: Sum of calculated irrigation demand for ET calculation. Depending on irrigation type and fraction of wetted surface, the netto amount for the whole area may be lower lower than applied irrigation water. */
        public Double? autoNetIrrigationEtc { get; set; }
        /*! interceptionAutoIrrEtc[mm], unit: "mm", description: Additional interception for the automated irrigated water. */
        public Double? interceptionAutoIrrEtc { get; set; }
        /*! balanceErrorEtc[mm], unit: "mm", description: Overall balance error for ET calculation between input water and output. This value must be near zero. The equation is: "precipitation + netIrrigation + drDiffEtc - interception - interceptionIrr - etAct - dpDzEtc + autoNetIrrigationEtc - interceptionAutoIrrEtc". If larger than 1E-3 the balance has gaps. */
        public Double? balanceErrorEtc { get; set; }
        /*! kcIni, unit: "none", description: Result from kcIni model, calculated crop coefficient for initial plant stage. */
        public Double? kcIni { get; set; }
        /*! ksMeanTcInitial, unit: "none", description: Mean value for water stress factor "Ks" in initial stage of plant growing for E plus T calculation. */
        public Double? ksMeanTcInitial { get; set; }
        /*! ksMeanTcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for E plus T calculation. */
        public Double? ksMeanTcDevelopment { get; set; }
        /*! ksMeanTcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for E plus T calculation. */
        public Double? ksMeanTcMid_season { get; set; }
        /*! ksMeanTcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for E plus T calculation. */
        public Double? ksMeanTcLate_season { get; set; }
        /*! ksMeanEtcInitial, unit: "none", description: Mean value for water stress factor "Ks" in initial stage of plant growing for ET calculation. */
        public Double? ksMeanEtcInitial { get; set; }
        /*! ksMeanEtcDevelopment, unit: "none", description: Mean value for water stress factor "Ks" in development stage of plant growing for ET calculation. */
        public Double? ksMeanEtcDevelopment { get; set; }
        /*! ksMeanEtcMid_season, unit: "none", description: Mean value for water stress factor "Ks" in mid-season stage of plant growing for ET calculation. */
        public Double? ksMeanEtcMid_season { get; set; }
        /*! ksMeanEtcLate_season, unit: "none", description: Mean value for water stress factor "Ks" in late-season stage of plant growing for ET calculation. */
        public Double? ksMeanEtcLate_season { get; set; }
        /*! yieldReductionTc, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public Double? yieldReductionTc { get; set; }
        /*! yieldReductionTcInitial, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for E plus T calculation. */
        public Double? yieldReductionTcInitial { get; set; }
        /*! yieldReductionTcDevelopment, unit: "none", description: Yield reduction factor due to water stress in development stage of plant growing for E plus T calculation. */
        public Double? yieldReductionTcDevelopment { get; set; }
        /*! yieldReductionTcMid_season, unit: "none", description: Yield reduction factor due to water stress in mid_season stage of plant growing for E plus T calculation. */
        public Double? yieldReductionTcMid_season { get; set; }
        /*! yieldReductionTcLate_season, unit: "none", description: Yield reduction factor due to water stress in late_season stage of plant growing for E plus T calculation. */
        public Double? yieldReductionTcLate_season { get; set; }
        /*! yieldReductionEtc, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for ET calculation. */
        public Double? yieldReductionEtc { get; set; }
        /*! yieldReductionEtcInitial, unit: "none", description: Yield reduction factor due to water stress in initial stage of plant growing for ET calculation. */
        public Double? yieldReductionEtcInitial { get; set; }
        /*! yieldReductionEtcDevelopment, unit: "none", description: Yield reduction factor due to water stress in development stage of plant growing for ET calculation. */
        public Double? yieldReductionEtcDevelopment { get; set; }
        /*! yieldReductionEtcMid_season, unit: "none", description: Yield reduction factor due to water stress in mid_season stage of plant growing for ET calculation. */
        public Double? yieldReductionEtcMid_season { get; set; }
        /*! yieldReductionEtcLate_season, unit: "none", description: Yield reduction factor due to water stress in late_season stage of plant growing for ET calculation. */
        public Double? yieldReductionEtcLate_season { get; set; }
        /*! dailyValues, description: Each line contains values for one day from "seedDate" to "harvestDate". This includes input values, intermediate results, evaporation, transpiration, evapotranspiration and soil water balance. */
        public IDictionary<DateTime, TranspirationResult> dailyValues { get; set; }
        /*! kcIniResult, description: Intermediate values and result of the "kcIni" model. */
        public KcIniResult kcIniResult { get; set; }
    }


    public class IrrigationModule
    {
        public TranspirationResult transpirationCalc(
            Plant plant,
            DateTime seedDate,
            DateTime harvestDate
            )
        {
            var result = new TranspirationResult();
            result.tAct = plant.stageTotal;
            return result;
        }
    }
}
