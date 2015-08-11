using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using atbApi.data;

namespace atbApi
{
    interface IAtbApi
    {
        ICollection<String> getPlantNames();
        Plant createPlant(String name);
        int transpirationCalc(Plant plant, DateTime seedDate, DateTime harvestDate);
    }
}
