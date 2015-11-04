using System;
using System.Numerics;

namespace Glyssen.Utilities
{
	public static class MathUtilities
	{
		/// <summary>
		/// Obtain a percentage as an int given two ints.
		/// </summary>
		/// <param name="numerator"></param>
		/// <param name="denominator"></param>
		/// <param name="maxPercent"></param>
		/// <returns></returns>
		public static int Percent(int numerator, int denominator, int maxPercent = 100)
		{
			return (int)PercentAsDouble(numerator, denominator, maxPercent);
		}

		/// <summary>
		/// Obtain a percentage as a double given two ints.
		/// </summary>
		/// <param name="numerator"></param>
		/// <param name="denominator"></param>
		/// <param name="maxPercent"></param>
		/// <returns></returns>
		public static double PercentAsDouble(int numerator, int denominator, int maxPercent = 100)
		{
			if (denominator == 0)
				return maxPercent == 0 ? 100 : maxPercent;
			if (maxPercent == 0)
				return ((double)numerator / denominator) * 100;
			return Math.Min(maxPercent, ((double)numerator / denominator) * 100);
		}

		public static BigInteger Factorial(int n)
		{
			if (n == 0) return 1;
			BigInteger ans = 1;
			for (int i = 1; i <= n; ++i)
				ans *= i;
			return ans;
		}

		public static BigInteger Power(int m, int p)
		{
			// m raised to the pth power
			BigInteger result = 1;
			for (int i = 0; i < p; ++i)
				result = result * m;
			return result;
		}

		public static BigInteger Choose(int n, int k)
		{
			if (n < 0 || k < 0)
				throw new Exception("Negative argument in Choose");
			if (n < k) return 0; // special
			if (n == k) return 1; // short-circuit

			int delta, iMax;

			if (k < n - k) // ex: Choose(100,3)
			{
				delta = n - k;
				iMax = k;
			}
			else           // ex: Choose(100,97)
			{
				delta = k;
				iMax = n - k;
			}

			BigInteger ans = delta + 1;
			for (int i = 2; i <= iMax; ++i)
				ans = (ans * (delta + i)) / i;

			return ans;
		}

		public static BigInteger Stirling(int n, int k)
		{
			BigInteger sum = 0;

			for (int j = 0; j <= k; ++j)
			{
				BigInteger a = Power(-1, k - j);
				BigInteger b = Choose(k, j);
				BigInteger c = Power(j, n);
				sum += a * b * c;
			}

			return sum / Factorial(k);
		}
	}
}
