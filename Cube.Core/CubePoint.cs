using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard implementation of <see cref="ICubePoint{TValueType}"/> for use with the <see cref="Cube.Dictionary.HybridDictionary{TValueType}"/>
	/// data structure.
	/// </summary>
	/// <remarks>This implementation does the unpacking between the <see cref="uint"/> data type used by the
	/// underlying collection and then the reflation into the actual values as desired through the consuming
	/// interface</remarks>
	/// <typeparam name="TValueType">The type of the cube value</typeparam>
	internal class CubePoint<TValueType> : ICubePoint<TValueType> where TValueType : ICubeValue
	{
		#region Member variables

		private readonly ValueMapper _valueMapper;
		private readonly KeyValuePair<uint [], TValueType> _kvp;

		#endregion

		#region Construction Logic

		public CubePoint( ValueMapper valueMapper, KeyValuePair<uint [], TValueType> kvp )
		{
			if( valueMapper == null )
				throw new ArgumentNullException( nameof( valueMapper ) );

			this._valueMapper = valueMapper;
			this._kvp = kvp;
		}

		#endregion

		#region ICubePoint Members

		public TValueType CubeValue
		{
			get
			{
				return this._kvp.Value;
			}
		}

		public object GetAxisValue( int axisIdx )
		{
			if( axisIdx < 0 || axisIdx >= _kvp.Key.Length )
				throw new ArgumentOutOfRangeException( nameof( axisIdx ), $"Invalid argument index specified, acceptable range is 0 - {_kvp.Key.Length}" );

			return this._valueMapper.GetObject( _kvp.Key [ axisIdx ] );
		}

		#endregion
	}
}
