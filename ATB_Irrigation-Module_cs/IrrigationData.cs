/*!
 * \file    IrrigationData.cs
 *
 * \brief	Class file for irrigation types and values
 * \author	Hunstock
 * \date     15.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace atbApi
{
    namespace data
    {

        /*! \brief	values depending on type of the irrigation. */
        public class IrrigationType
        {
            private double _fw;
            private double _interception;
            private String _name;

            /*! readonly property: fraction of wetted surface for irrigation method, between 0.3 (30% wetted surface for drip) up to 1 (100% for sprinkler) */
            public double fw { get { return this._fw; } }
            /*! readonly property: interception as result of the method, if leafs are not wetted (for instance drip irrigation), this value is 0 */
            public double interception { get { return this._interception; } }
            /*! readonly property: name of the irrigation method */
            public String name { get { return this._name; } }

            /*!
             * \brief   Constructor for new struct, all values must be provided.
             *
             * \param   fw              The fraction of wetted surface.
             * \param   interception    The factor for interception calculation.
             * \param   name            The name of the irrigation method.
             */

            public IrrigationType(double fw, double interception, String name)
            {
                this._fw = fw;
                this._interception = interception;
                this._name = name;
            }
        }

        /*!
         * \brief   List of all in FAO paper 56 defined irrigation types to consider fraction of wetted surface and interception for each method.
         *
         */

        public struct IrrigationTypes {
            /*! Irrigation method trickle, same like drip, but here with fw = 0.4 */
            public static readonly IrrigationType trickle = new IrrigationType(0.4, 0, "trickle");
            /*! Irrigation method furrow with narrow beds, more surface is wetted. */
            public static readonly IrrigationType furrow_narrow_bed = new IrrigationType(0.8, 0, "furrow_narrow_bed");
            /*! Irrigation method furrow with wide beds, less surface is wetted. */
            public static readonly IrrigationType furrow_wide_bed = new IrrigationType(0.5, 0, "furrow_wide_bed");
            /*! Irrigation method furrow, but not all at once, fw is reduced. */
            public static readonly IrrigationType furrow_alternated = new IrrigationType(0.4, 0, "furrow_alternated");
            /*! Irrigation method sprinkler, is calculated like precipitation from above. */
            public static readonly IrrigationType sprinkler = new IrrigationType(1, 1, "sprinkler");
            /*! Irrigation method basin, whole area is flooded and the irrigation water stays in the basin. */
            public static readonly IrrigationType basin = new IrrigationType(1, 0, "basin");
            /*! Irrigation method border, borders are regulated to part time flood the field. */
            public static readonly IrrigationType border = new IrrigationType(1, 0, "border");
            /*! Irrigation method drip, water is applied nearby the plants and the roots. */
            public static readonly IrrigationType drip = new IrrigationType(0.3, 0, "drip");
        }

        /*!
         * \brief   An irrigation schedule. This class is used to pass irrigation data to and from this dll.
         *
         */

        public class IrrigationSchedule
        {
            /*!
             * \brief   Irrigation schedule as HashMap of Dates and irrigation amounts
             *
             */

            public IDictionary<DateTime, double> schedule { get; set; }

            /*!
             * Default constructor, an empty schedule is created.
             *
             */

            public IrrigationSchedule()
            {
                this.schedule = new Dictionary<DateTime, double>();
            }
        }
    }
}
