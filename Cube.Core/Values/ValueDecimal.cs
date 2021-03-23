using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.Cube.Values
{
	/// <summary>
	/// Standard implementation of <see cref="ICubeValue"/> for wrapping a decimal
	/// </summary>
	public class ValueDecimal : ICubeValue
	{
		public decimal Value { get; set; }

		public ICubeValue Clone()
		{
			return new ValueDecimal()
			{
				Value = this.Value
			};
		}

		public void Add(ICubeValue cubeValue)
		{
			if (cubeValue == null)
				throw new ArgumentNullException(nameof(cubeValue));

			if (!(cubeValue is ValueDecimal cubeValueAsValueDecimal))
				throw new InvalidOperationException($"Unable to add a non-ValueDecimal instance to this. Added type -{cubeValue.GetType().Name}");

			this.Value += cubeValueAsValueDecimal.Value;
		}
	}
}
