using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.Cube.Values
{
	/// <summary>
	/// Standard implementation of <see cref="ICubeValue"/> for wrapping a double
	/// </summary>
	public class ValueDouble : ICubeValue
	{
		public double Value { get; set; }

		public ICubeValue Clone()
		{
			return new ValueDouble()
			{
				Value = this.Value
			};
		}

		public void Add(ICubeValue cubeValue)
		{
			if (cubeValue == null)
				throw new ArgumentNullException(nameof(cubeValue));

			if (!(cubeValue is ValueDouble cubeValueAsValueDouble))
				throw new InvalidOperationException($"Unable to add a non-ValueDouble instance to this. Added type -{cubeValue.GetType().Name}");

			this.Value += cubeValueAsValueDouble.Value;
		}
	}
}
