using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube.Values
{
	[TestClass]
	public class ValueFloatTests
	{
		[TestMethod]
		public void CloneWorks()
		{
			var val = new ValueFloat
			{
				Value = 2.3f
			};

			var clone = val.Clone();

			clone.Should().BeEquivalentTo(val);
		}

		[TestMethod]
		public void AddWorks()
		{
			var val1 = new ValueFloat
			{
				Value = 2.3f
			};

			var val2 = new ValueFloat
			{
				Value = 4.3f
			};

			var expectedResult = val1.Value + val2.Value;

			val1.Add(val2);

			Assert.AreEqual(expectedResult, val1.Value);
		}

		[TestMethod]
		public void AddFailsOnDifferentType()
		{
			var val1 = new ValueFloat
			{
				Value = 2.3f
			};

			var val2 = new ValueDouble
			{
				Value = 4.3
			};

			Assert.ThrowsException<InvalidOperationException>(() => val1.Add(val2));
		}
	}
}
