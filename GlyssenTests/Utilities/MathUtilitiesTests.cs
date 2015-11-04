using System;
using System.Numerics;
using Glyssen.Utilities;
using NUnit.Framework;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	public class MathUtilitiesTests
	{
		[Test]
		public void Percent_ResultCalculatesCorrectly()
		{
			Assert.AreEqual(50, MathUtilities.Percent(1, 2));
		}

		[Test]
		public void PercentAsDouble_ResultCalculatesCorrectly()
		{
			Assert.AreEqual(50d, MathUtilities.PercentAsDouble(1, 2));
		}

		[Test]
		public void PercentAsDouble_ResultNotMoreThanMax()
		{
			int max = 99;
			Assert.AreEqual((double)max, MathUtilities.PercentAsDouble(1, 1, max));
		}

		[Test]
		public void PercentAsDouble_AllowsResultGreaterThan100()
		{
			Assert.AreEqual(200d, MathUtilities.PercentAsDouble(2, 1, 0));
		}

		[Test]
		public void PercentAsDouble_DivideBy0ReturnsMaxOr100()
		{
			Assert.AreEqual(99d, MathUtilities.PercentAsDouble(1, 0, 99));
			Assert.AreEqual(100d, MathUtilities.PercentAsDouble(1, 0, 0));
			Assert.AreEqual(100d, MathUtilities.PercentAsDouble(1, 0));
		}

		[Test]
		public void Stirling_kGreaterThanN_ReturnsZero()
		{
			int n = 1;
			int k = 2;
			Assert.AreEqual(BigInteger.Zero, MathUtilities.Stirling(n, k));
		}

		[TestCase(0, 0, 1)]
		[TestCase(1, 0, 0)]
		[TestCase(100, 0, 0)]
		[TestCase(1, 1, 1)]
		[TestCase(2, 2, 1)]
		[TestCase(2, 1, 1)]
		[TestCase(1000, 1, 1)]
		[TestCase(3, 2, 3)]
		[TestCase(7, 3, 301)]
		[TestCase(10, 5, 42525)]
		[TestCase(300, 20, 42525)]
		public void Stirling_ReturnsCorrectResults(int n, int k, int expectedResult)
		{
			Assert.AreEqual(new BigInteger(expectedResult), MathUtilities.Stirling(n, k));
		}
	}
}
