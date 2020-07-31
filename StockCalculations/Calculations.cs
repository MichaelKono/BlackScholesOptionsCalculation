using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RootFinding;

namespace StockCalculations
{
    public class Calculations
    {
        public enum OptionContractType
        {
            Call = 1,
            Put = 2
        }

        private static double D1(double S, double K, double T, double sigma, double r, double q)
        {
            return (Math.Log(S / K) + (r - q + (sigma * sigma) / 2) * T) / (sigma * Math.Sqrt(T));
        }

        private static double D2(double T, double sigma, double d1)
        {
            return d1 - sigma * Math.Sqrt(T);
        }
        public static double T(DateTimeOffset contractExpirationTime, DateTimeOffset time)
        {
            if (time > contractExpirationTime)
                throw new ArgumentOutOfRangeException(nameof(time));

            return (contractExpirationTime - time).TotalDays / 365.0;
        }


        /// <summary>
        /// Computes theoretical price.
        /// </summary>
        /// <param name="optionType">call or put</param>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Premium(OptionContractType optionType, double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);
            double d2 = D2(T, sigma, d1);

            switch (optionType)
            {
                case OptionContractType.Call:
                    return S * Math.Exp(-q * T) * Normal.CDF(0, 1, d1) - K * Math.Exp(-r * T) * Normal.CDF(0, 1, d2);

                case OptionContractType.Put:
                    return K * Math.Exp(-r * T) * Normal.CDF(0, 1, -d2) - S * Math.Exp(-q * T) * Normal.CDF(0, 1, -d1);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Computes Vega. The amount of option price change for each 1% change in vol (sigma)
        /// </summary>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Vega(double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);
            double vega = S * Math.Exp(-q * T) * Normal.PDF(0, 1, d1) * Math.Sqrt(T);
            return vega / 100;
        }

        public double IV(OptionContractType optionType, double S, double K, double T, double r, double q, double optionMarketPrice)
        {
            Func<double, double> f = sigma => Premium(optionType, S, K, T, sigma, r, q) - optionMarketPrice;
            Func<double, double> df = sigma => Vega(S, K, T, sigma, r, q);

            return RobustNewtonRaphson.FindRoot(f, df, lowerBound: 0, upperBound: 100, accuracy: 0.001);
        }

        /// <summary>
        /// Computes theta.
        /// </summary>
        /// <param name="optionType">call or put</param>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Theta(OptionContractType optionType, double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);
            double d2 = D2(T, sigma, d1);

            switch (optionType)
            {
                case OptionContractType.Call:
                    {

                        double theta = -Math.Exp(-q * T) * (S * Normal.PDF(0, 1, d1) * sigma) / (2.0 * Math.Sqrt(T))
                                - (r * K * Math.Exp(-r * T) * Normal.CDF(0, 1, d2))
                                + q * S * Math.Exp(-q * T) * Normal.CDF(0, 1, d1);

                        return theta / 365;
                    }

                case OptionContractType.Put:
                    {
                        double theta = -Math.Exp(-q * T) * (S * Normal.PDF(0, 1, d1) * sigma) / (2.0 * Math.Sqrt(T))
                            + (r * K * Math.Exp(-r * T) * Normal.PDF(0, 1, -d2))
                            - q * S * Math.Exp(-q * T) * Normal.CDF(0, 1, -d1);

                        return theta / 365;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Computes delta.
        /// </summary>
        /// <param name="optionType">call or put</param>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Delta(OptionContractType optionType, double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);

            switch (optionType)
            {
                case OptionContractType.Call:
                    return Math.Exp(-r * T) * Normal.CDF(0, 1, d1);

                case OptionContractType.Put:
                    return -Math.Exp(-r * T) * Normal.CDF(0, 1, -d1);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Computes gamma.
        /// </summary>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Gamma(double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);
            return Math.Exp(-q * T) * (Normal.PDF(0, 1, d1) / (S * sigma * Math.Sqrt(T)));
        }

        /// <summary>
        /// Computes delta.
        /// </summary>
        /// <param name="optionType">call or put</param>
        /// <param name="S">Underlying price</param>
        /// <param name="K">Strike price</param>
        /// <param name="T">Time to expiration in % of year</param>
        /// <param name="sigma">Volatility</param>
        /// <param name="r">continuously compounded risk-free interest rate</param>
        /// <param name="q">continuously compounded dividend yield</param>
        /// <returns></returns>
        public double Rho(OptionContractType optionType, double S, double K, double T, double sigma, double r, double q)
        {
            double d1 = D1(S, K, T, sigma, r, q);
            double d2 = D2(T, sigma, d1);

            switch (optionType)
            {
                case OptionContractType.Call:
                    return K * T * Math.Exp(-r * T) * Normal.CDF(0, 1, d2);

                case OptionContractType.Put:
                    return -K * T * Math.Exp(-r * T) * Normal.CDF(0, 1, -d2);

                default:
                    throw new NotSupportedException();
            }
        }

    }
}
