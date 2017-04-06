/*!
 * \file    MeanValue.cs
 *
 * \brief   Implements a mean value to simply collect values and access the mean as a property.
 *
 * \author  Hunstock
 * \date    06.03.2017
 */

namespace atbApi
{

    /*! 
     * \brief   namespace for public available classes and structures
     * 
     */
    namespace tools
    {
        /*!
         * \brief   A mean value class.
         *
         */

        public class MeanValue
        {
            private int num = 0;
            private double sum = 0;

            /*!
             * \brief   Gets the value.
             *
             * \return  The mean value.
             */

            public double value
            {
                get
                {
                    if (num == 0) return 0;
                    return sum / num;
                }
            }

            /*!
             * \brief   Adds a value to the mean.
             *
             * \param   value   The value to add.
             */

            public void Add(double value)
            {
                sum += value;
                num++;
            }
        }
    }
}
