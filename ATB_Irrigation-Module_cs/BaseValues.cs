/*!
 * \file    BaseValues.cs
 *
 * \brief   Implements the base values class for data that have to be readed and parsed from csv data streams.
 *
 * \author	Hantigk
 * \date	13.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace atbApi
{
    /*! 
     * \brief   namespace where all exported classes and structures for data access are contained
     * 
     */
    namespace data
    {
        /*!
         * \brief   The base values class, base class for plant- and soil data. Provides useful functions to parse csv data.
         *
         */

        public abstract class BaseValues
        {
            private static IDictionary<String, String> emptyMap = new Dictionary<String, String>();

            protected void parseData(IDictionary<String, String> values)
            {
                parseData(values, emptyMap);
            }

            protected void parseData(IDictionary<String, String> values, PlantDb pdb = null, SoilDb sdb = null, ClimateDb cdb = null)
            {
                parseData(values, emptyMap, pdb, sdb, cdb);
            }

            /*!
             * \brief   Parse data, iterates through all properties with getter and setter and tries to set with values
             * 
             * \param   values      The csv values.
             * \param   nameDict    Dictionary of names. 'property name' => 'csv-field name'
             */

            protected void parseData(IDictionary<String, String> values, IDictionary<String, String> nameDict, PlantDb pdb = null, SoilDb sdb = null, ClimateDb cdb = null)
            {
                //use this for .net 4.0
                //foreach (PropertyInfo pi in this.GetType().GetProperties())
                //use this for .net 4.5
                foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
                {
                    //if (pi.GetGetMethod() == null || pi.GetSetMethod() == null) continue;

                    String name = nameDict.ContainsKey(pi.Name) ? nameDict[pi.Name] : pi.Name;
                    if (!values.ContainsKey(name)) continue;

                    String value = values[name];
                    if (String.IsNullOrWhiteSpace(value) || value.StartsWith("#NV")) continue;

                    Type type = Nullable.GetUnderlyingType(pi.PropertyType);
                    if (type == null) type = pi.PropertyType;

                    if (type == typeof(String))
                    {
                        pi.SetValue(this, value, null);
                    }
                    else if (type == typeof(Double))
                    {
                        pi.SetValue(this, Double.Parse(value, CultureInfo.InvariantCulture), null);
                    }
                    //use this for .net 4.0
                    //else if (type.IsEnum)
                    //use this for .net 4.5
                    else if (type.GetTypeInfo().IsEnum)
                    {
                        try
                        {
                            pi.SetValue(this, System.Enum.Parse(type, value), null);
                        }
                        catch
                        {
                            pi.SetValue(this, null, null);
                        }
                    }
                    else if (type == typeof(Plant))
                    {
                        if (pdb == null) continue;
                        pi.SetValue(this, pdb.getPlant(value), null);
                    }
                    else if (type == typeof(Soil))
                    {
                        if (sdb == null) continue;
                        pi.SetValue(this, sdb.getSoil(value), null);
                    }
                    else if (type == typeof(Climate))
                    {
                        if (cdb == null) continue;
                        pi.SetValue(this, cdb.getClimate(value), null);
                    }
                    else if (type == typeof(IrrigationType))
                    {
                        foreach (FieldInfo fi in typeof(IrrigationTypes).GetRuntimeFields()) {
                            if (fi.Name == value)
                            {
                                IrrigationType tmpIt = new IrrigationType();
                                tmpIt = (IrrigationType) fi.GetValue(tmpIt);
                                pi.SetValue(this, tmpIt, null);
                                break;
                            }
                        }
                    }
                    else /* if (type.isValueType)*/
                    {
                        pi.SetValue(this, Convert.ChangeType(value, type), null);
                    }
                }
            }
        }
    }
}
