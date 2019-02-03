using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing a value within a cube structure
	/// </summary>
	public interface ICubeValue
	{
		/// <summary>
		/// Clone the current objects
		/// </summary>
		/// <returns>A clone of the current value</returns>
		ICubeValue Clone();

		/// <summary>
		/// Adds the given value to this item
		/// </summary>
		/// <param name="cubeValue">The value to be added</param>
		void Add( ICubeValue cubeValue );
	}
}
