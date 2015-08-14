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

namespace atbApi
{

    public class IrrigationModule
    {
        public int transpirationCalc(Plant plant, DateTime seedDate, DateTime harvestDate)
        {
            return plant.stageTotal;
        }
    }
}
