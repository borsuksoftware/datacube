using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing an error set within the cube framework
	/// </summary>
	public interface IErrorSet : IEnumerable<IError>
	{
		/// <summary>
		/// Gets the set of errors which match the given set of input keys
		/// </summary>
		/// <param name="errorKeys">A dictionary which contains the set of cube points to match the error on</param>
		/// <returns>An enumerator containing the appropriate set of errors</returns>
		IEnumerator<IError> GetMatchingErrors( IDictionary<string, object> errorKeys );
	}
}
