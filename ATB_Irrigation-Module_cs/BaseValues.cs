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
         * \brief   The base values class, base class for climate, cropSequence plant and soil data. Provides useful functions to parse csv data.
         *
         */

        public abstract class BaseValues
        {
            /*!
             * \brief   Parse readed csv key-value pairs to real data types.
             *          Iterates through all properties with getter and setter and tries to set with value.
             *
             * \param   values      The csv values.
             * \param   nameDict    Dictionary to translate names. 'property name' => 'csv-field name'.
             * \param   pdb         The plant database to parse type "Plant".
             * \param   sdb         The soil database to parse type "Soil".
             * \param   cdb         The climate database to parse type "Climate".
             * \param   cultureInfo Information to parse data in different localized formats
             */

            protected void parseData(IDictionary<String, String> values, IDictionary<String, String> nameDict = null, PlantDb pdb = null, SoilDb sdb = null, ClimateDb cdb = null, RainPatternDb rpdb = null, CultureInfo cultureInfo = null)
            {
                IEnumerable<PropertyInfo> piList = this.GetType().GetRuntimeProperties();

                IList<String> propertyNames = new List<String>();
                foreach (PropertyInfo pi in piList) propertyNames.Add(pi.Name);

                IDictionary<String, IDictionary<String, String>> subObjects = new Dictionary<String, IDictionary<String, String>>();
                foreach (String key in values.Keys)
                {
                    // Skip known properties
                    if (propertyNames.Contains(key)) continue;
                    
                    if (key.Contains("."))
                    {
                        String[] parts = key.Split(".".ToCharArray(), 2);

                        String subObjectName = parts[0];
                        String subObjectProperty = parts[1];
                        
                        // FIXME: Raise error for empty parts
                        if (
                            String.IsNullOrWhiteSpace(subObjectName) ||
                            String.IsNullOrWhiteSpace(subObjectProperty) ||
                            subObjectName.Equals("_iterator") ||
                            !propertyNames.Contains(subObjectName)
                        ) continue;

                        if (!subObjects.ContainsKey(subObjectName)) subObjects.Add(subObjectName, new Dictionary<String, String>());
                        subObjects[subObjectName].Add(subObjectProperty, values[key]);
                    }
                }
                foreach (String subObjName in subObjects.Keys)
                {
                    PropertyInfo pi = this.GetType().GetRuntimeProperty(subObjName);

                    if (pi.PropertyType.IsArray)
                    {
                        Type arrayType = pi.PropertyType.GetElementType();
                        List<int> indexes = subObjects[subObjName].Keys.Select(Int32.Parse).ToList();
                        Array arrayInstance = Array.CreateInstance(arrayType, indexes.Max() + 1);

                        foreach (int index in indexes)
                        {
                            if (String.IsNullOrEmpty(subObjects[subObjName][index.ToString()])) continue;

                            if (arrayType == typeof(String))
                            {
                                arrayInstance.SetValue(subObjects[subObjName][index.ToString()], index);
                            }
                            else if (arrayType == typeof(Double))
                            {
                                arrayInstance.SetValue(Double.Parse(subObjects[subObjName][index.ToString()], cultureInfo != null ? cultureInfo : CultureInfo.InvariantCulture), index);
                            }
                            else if (arrayType == typeof(int) || arrayType == typeof(Int16) || arrayType == typeof(Int32) || arrayType == typeof(Int64))
                            {
                                arrayInstance.SetValue(int.Parse(subObjects[subObjName][index.ToString()], cultureInfo != null ? cultureInfo : CultureInfo.InvariantCulture), index);
                            }
                        }

                        pi.SetValue(this, arrayInstance, null);
                    }
                    else
                    {
                        BaseValues instance = Activator.CreateInstance(pi.PropertyType) as BaseValues;
                        // FIXME: Raise error on wrong object type
                        if (instance == null) continue;

                        instance.parseData(subObjects[subObjName], nameDict, pdb, sdb, cdb, rpdb, cultureInfo);
                        pi.SetValue(this, instance, null);
                    }

                }

                //use this for .net 4.0
                //foreach (PropertyInfo pi in this.GetType().GetProperties())
                //use this for .net 4.5
                foreach (PropertyInfo pi in piList)
                {
                    //if (pi.GetGetMethod() == null || pi.GetSetMethod() == null) continue;

                    String name = nameDict != null && nameDict.ContainsKey(pi.Name) ? nameDict[pi.Name] : pi.Name;
                    if (!values.ContainsKey(name)) continue;

                    propertyNames.Add(name);

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
                        pi.SetValue(this, Double.Parse(value, cultureInfo != null ? cultureInfo : CultureInfo.InvariantCulture), null);
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
                    else if (type == typeof(RainPattern))
                    {
                        if (rpdb == null) continue;
                        pi.SetValue(this, rpdb.getRainPattern(value), null);
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
