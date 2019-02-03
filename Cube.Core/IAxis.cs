using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing an axis within the cube
	/// </summary>
	public interface IAxis
	{
		/// <summary>
		/// The name of the axis
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The type of the axis
		/// </summary>
		Type DataType { get; }

		/// <summary>
		/// Gets whether or not the axis allows being totalled
		/// </summary>
		bool AllowTotals { get; }
	}
}
