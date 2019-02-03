using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing a point within a data cube
	/// </summary>
	/// <typeparam name="TValueType">The type of values for this point</typeparam>
	/// <seealso cref="ICubeValue"/>
	public interface ICubePoint<TValueType> where TValueType : ICubeValue
	{
		/// <summary>
		/// Gets the value for the specified axis
		/// </summary>
		/// <param name="axisIdx">The index of the axis that's desired</param>
		/// <returns>The object</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid <paramref name="axisIdx"/> is specified</exception>
		object GetAxisValue( int axisIdx );

		/// <summary>
		/// Gets the value that this cube represents
		/// </summary>
		TValueType CubeValue { get; }
	}
}
