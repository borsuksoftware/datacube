using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.Cube.Values
{
	/// <summary>
	/// Standard implementation of <see cref="ICubeValue"/> for wrapping a float
	/// </summary>
	public class ValueFloat : ICubeValue
	{
		public float Value { get; set; }

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

			if (!(cubeValue is ValueFloat cubeValueAsValueFloat))
				throw new InvalidOperationException($"Unable to add a non-ValueFloat instance to this. Added type -{cubeValue.GetType().Name}");

			this.Value += cubeValueAsValueFloat.Value;
		}
	}
}
