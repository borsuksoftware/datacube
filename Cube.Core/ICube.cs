using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Interface representing how a data cube can be consumed
	/// </summary>
	public interface ICube<TValueType> :
		IEnumerable<ICubePoint<TValueType>>
		where TValueType : ICubeValue
	{
		/// <summary>
		/// Tries to get the value for the given set of keys
		/// </summary>
		/// <param name="keys">The keys to look for</param>
		/// <param name="cubeValue">The cube value if found</param>
		/// <returns>True if the value was found, otherwise false</returns>
		bool TryGetValue( object [] keys, out TValueType cubeValue );

		/// <summary>
		/// Gets the error set for this cube
		/// </summary>
		/// <value>The error set for this cube</value>
		IErrorSet ErrorSet { get; }
	}
}
