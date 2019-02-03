using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing an error within the cube infrastructure
	/// </summary>
	/// <remarks>
	/// Errors in general can be specified at any level of granularity at or below that of the cube, i.e. 
	/// A single error could be defined as for all possible values of an axis (e.g. ccy, tenor) for a known set 
	/// of other axes (Trade ID). This approach reduces the total volume of errors that need to be stored in
	/// the set, especially when the fact that the universe of possible axis values (e.g. ccy, tenor) isn't known
	/// in the case of an evaluation error
	/// </remarks>
	public interface IError
	{
		/// <summary>
		/// Gets the set of axes that this error was defined for
		/// </summary>
		ICollection<string> ErrorAxes { get; }

		/// <summary>
		/// Gets the key for the specified axis that there's an error for
		/// </summary>
		/// <param name="axisName"></param>
		/// <returns></returns>
		object GetErrorKey( string axisName );

		/// <summary>
		/// Gets the message for this error
		/// </summary>
		string ErrorMessage { get; }
	}
}
