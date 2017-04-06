/*!
 * \file    IrrigationData.cs
 *
 * \brief	Class file for irrigation types and values
 *
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
        /*!
         *  \brief  class holding all values that determines an irrigation type.
         *
         */

        public class IrrigationType
        {
            private double _fw;
            private double _interception;
            private String _name;
            private double _min;
            private double _max;

            /*! readonly property: fraction of wetted surface for irrigation method, between 0.3 (30% wetted surface for drip) up to 1 (100% for sprinkler) */
            public double fw { get { return this._fw; } }
            /*! readonly property: interception as result of the method, if leafs are not wetted (for instance drip irrigation), this value is 0 */
            public double interception { get { return this._interception; } }
            /*! readonly property: name of the irrigation method */
            public String name { get { return this._name; } }
            /*! readonly property: minimum irrigation application per day */
            public double min { get { return this._min; } }
            /*! readonly property: maximum irrigation application per day */
            public double max { get { return this._max; } }

            /*!
             * \brief   Constructor for new struct, all values must be provided.
             *
             *
             * \param   fw              The fraction of wetted surface.
             * \param   interception    The factor for interception calculation.
             * \param   name            The name of the irrigation method.
             * \param   min             The minimum of applied irrigation per event (day).
             * \param   max             The maximum of applied irrigation per event (day).
             */

            public IrrigationType(double fw = 1, double interception = 1, String name = "sprinkler", double min = 0, double max = 0)
            {
                this._fw = fw;
                this._interception = interception;
                this._name = name;
                this._min = min;
                this._max = max;
            }
        }

        /*!
         * \brief   List of all in FAO paper 56 defined irrigation types to consider fraction of wetted surface and interception for each method.
         *
         */

        public struct IrrigationTypes {
            /*! Irrigation method trickle, same like drip, but here with fw = 0.4 */
            public static readonly IrrigationType trickle = new IrrigationType(0.4, 0, "trickle", 3, 50);
            /*! Irrigation method furrow with narrow beds, more surface is wetted. */
            public static readonly IrrigationType furrow_narrow_bed = new IrrigationType(0.8, 0, "furrow_narrow_bed", 10, 100);
            /*! Irrigation method furrow with wide beds, less surface is wetted. */
            public static readonly IrrigationType furrow_wide_bed = new IrrigationType(0.5, 0, "furrow_wide_bed", 10, 100);
            /*! Irrigation method furrow, but not all at once, fw is reduced. */
            public static readonly IrrigationType furrow_alternated = new IrrigationType(0.4, 0, "furrow_alternated", 10, 50);
            /*! Irrigation method sprinkler, is calculated like precipitation from above. */
            public static readonly IrrigationType sprinkler = new IrrigationType(1, 1, "sprinkler", 10, 100);
            /*! Irrigation method basin, whole area is flooded and the irrigation water stays in the basin. */
            public static readonly IrrigationType basin = new IrrigationType(1, 0, "basin", 10, 100);
            /*! Irrigation method border, borders are regulated to part time flood the field. */
            public static readonly IrrigationType border = new IrrigationType(1, 0, "border", 10, 50);
            /*! Irrigation method drip, water is applied nearby the plants and the roots. */
            public static readonly IrrigationType drip = new IrrigationType(0.3, 0, "drip", 3, 50);
        }

        /*!
         * \brief   class that holds information how automatic irrigation is applied.
         *
         */

        public class AutoIrrigationControl
        {
            /*! If "autoIrrigation" is used, it will start at this fraction of available water (0.8 means 80%). If this value is 0, irrigations starts if saturation drops below "1 - pAdj" level */
            public double level { get; set; }
            /*! If "autoIrrigation" is used _and_ "autoIrrAmount" is 0 (automatic amount calculation), then the amount is calculated to saturate the soil right up to "autoIrrCutoff" value. If "autoIrrigation" is used _and_ "autoIrrAmount" is 0 _and_ "autoIrrLevel" is 0, then cutoff equation is "1 - pAdj - soilSaturation + autoIrrCutoff" */
            public double cutoff { get; set; }
            /*! If "autoIrrigation" is used, this amount of irrigation is added per day, if available soil water drops below "autoIrrLevel". If this value is 0, it is calculated by rule of "autoIrrCutoff" value */
            public double amount { get; set; }
            /*! If "autoIrrigation" is used _and_ "autoIrrAmount" is 0 _and_ "autoIrrLevel" is 0, then irrigation is started at "1 - pAdj - autoIrrDeficit" */
            public double deficit { get; set; }
            public IrrigationType type { get; set; }
            /*! Automatic irrigation start day, no irrigation is applied before this day of plant development */
            public Int32? startDay { get; set; }
            /*! Automatic irrigation end day, no irrigation is applied after this day of plant development */
            public Int32? endDay { get; set; }

            /*!
             * \brief   Default Constructor with optional named arguments
             * reasonable values for agriculture near Isfahan
             *
             * \param   level       The automatic irr level.
             * \param   cutoff      The automatic irr cutoff.
             * \param   amount      The automatic irr amount.
             * \param   deficit     The deficit.
             * \param   type        The type.
             * \param   startDay    The start day.
             * \param   endDay      The end day.
             */

            public AutoIrrigationControl(
                double level = 0,
                double cutoff = 0.15,
                double amount = 0,
                double deficit = 0.2,
                IrrigationType type = null,
                Int32? startDay = null,
                Int32? endDay = null
            )
            {
                this.level = level;
                this.cutoff = cutoff;
                this.amount = amount;
                this.deficit = deficit;
                this.type = type == null ? IrrigationTypes.sprinkler : type;
                this.startDay = startDay;
                this.endDay = endDay;
            }
        }


        /*!
         * \brief   An irrigation schedule. This class is used to pass irrigation data to and from this dll.
         *
         */

        public class IrrigationSchedule
        {
            /*! Irrigation schedule as HashMap of Dates and irrigation amounts in mm */
            public IDictionary<DateTime, double> schedule { get; set; }
            /*! Type of irrigation method, use IrrigationType class */
            public IrrigationType type { get; set; }

            /*!
             * \brief   Default constructor, irrgationType defaults to "sprinkler"
             *
             */

            public IrrigationSchedule()
                : this(IrrigationTypes.sprinkler)
            {
            }

            /*!
             * \brief   Constructor, an empty schedule is created, irrgationType is set
             *
             */

            public IrrigationSchedule(IrrigationType type)
            {
                this.schedule = new Dictionary<DateTime, double>();
                this.type = type;
            }
        }
    }
}
