using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing a set of axes within the cube framework
	/// </summary>
	public interface IAxisSet : IEnumerable<IAxis>
	{
		/// <summary>
		/// Gets the named axis if it exists
		/// </summary>
		/// <param name="axisName">The name of the axis to search for</param>
		/// <returns>The axis</returns>
		IAxis this [ string axisName ] { get; }

		/// <summary>
		/// Gets the axis at the specified index
		/// </summary>
		/// <remarks>If an invalid index is specified, then an exception will be thrown</remarks>
		/// <param name="axisIndex">The index that is being looked for</param>
		/// <returns>The axis</returns>
		IAxis this [ int axisIndex ] { get; }

		/// <summary>
		/// Gets the number of axes in this set
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Gets the index, if available, of the named axes
		/// </summary>
		/// <param name="axisName">The name of the axis</param>
		/// <returns>The index of the axis if available, otherwise -1</returns>
		int GetIndexOf( string axisName );
	}
}
