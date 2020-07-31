using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RootFinding;

namespace StockCalculations
{
    class BinomialPricing
    {

        public enum OptionContractType
        {
            Call = 1,
            Put = 2
        }

        /// <summary>
        /// Computes delta.
        /// </summary>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <param name="n">height of binomial tree</param>
        /// <returns></returns>
        public double BinomialPricingCalculation(OptionContractType optionType, double c1, double c2, double r)
        {
            var Pi = UpDownProbabability(10, 10, 10);
            
            

            switch (optionType)
            {

                case OptionContractType.Call:
                    return (Pi * c1 + ( 1 - Pi ) * c2 ) / (1 + r);

                case OptionContractType.Put:
                    return 0;

                default:
                    throw new NotSupportedException();

            }
        }

        private double UpFactor(double s)
        {
            return (s - 0.01) / s;
        }

        /// <summary>
        /// Calculates call max
        /// </summary>
        /// <param name="px">Excersie Price</param>
        /// <param name="s">Spot price</param>
        private double c1(double px, double s)
        {
            return Math.Max(0,px);
        }
        private double c2()
        {

            return 0;
        }

        private double UpDownProbabability(double r, double u, double d)
        {
            return ( 1 + r - d ) / ( u - d );
        }
    }
}
