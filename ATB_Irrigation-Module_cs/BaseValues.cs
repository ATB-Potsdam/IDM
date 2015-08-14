using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace atbApi
{
    public abstract class BaseValues
    {
        private static IDictionary<String, String> emptyMap = new Dictionary<String, String>();

        protected void parseData(IDictionary<String, String> values)
        {
            parseData(values, emptyMap);
        }

        /*!
         * Parse data.
         *
         * \author  Hunstock
         * \date    13.08.2015
         *
         * Iterates through all properties with getter and setter and tries to set with values
         * 
         * \param   values      The csv values.
         * \param   nameDict    Dictionary of names. 'property name' => 'csv-field name'
         */

        protected void parseData(IDictionary<String, String> values, IDictionary<String,String> nameDict) {
            foreach (PropertyInfo pi in this.GetType().GetProperties()) {
                //if (pi.GetGetMethod() == null || pi.GetSetMethod() == null) continue;

                String name = nameDict.ContainsKey(pi.Name) ? nameDict[pi.Name] : pi.Name;
                if (!values.ContainsKey(name)) continue;

                String value= values[name];
                if (String.IsNullOrWhiteSpace(value)) continue;

                Type type= Nullable.GetUnderlyingType(pi.PropertyType);
                if (type == null) type = pi.PropertyType;
                
                if (type == typeof(String)) {
                    pi.SetValue(this, value, null);
                }
                else if (type == typeof(Double))
                {
                    pi.SetValue(this, Double.Parse(value, CultureInfo.InvariantCulture), null);
                }
                else if (type.IsEnum)
                {
                    try
                    {
                        pi.SetValue(this, System.Enum.Parse(type, value), null);
                    }
                    catch {
                        pi.SetValue(this, null, null);
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
