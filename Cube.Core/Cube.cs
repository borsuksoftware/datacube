using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard implementation of <see cref="ICube{TValueType}"/>.
	/// </summary>
	/// <remarks>This implementation uses a tree based dictionary internally to reduce the 
	/// memory footprint of the data structure
	/// 
	/// <para>All read operations are guaranteed to be threadsafe, write operations are not and must be externally
	/// synced</para>
	/// </remarks>
	/// <typeparam name="TValueType">The value type for values within this structure</typeparam>
	public class Cube<TValueType> : ICube<TValueType> where TValueType : ICubeValue
	{
		#region Member variables

		private readonly ValueMapper _valueMapper = new ValueMapper();

		private readonly Dictionary.HybridDictionary<TValueType> _dictionary;

		private readonly ErrorSet _errorSet;

		#endregion

		#region Data Model

		public IAxisSet AxisSet { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of the <see cref="Cube{TValueType}"/> class.
		/// </summary>
		/// <param name="axisSet">The axis set to use</param>
		/// <exception cref="ArgumentNullException">Thrown if no value is supplied for <paramref name="axisSet"/></exception>
		/// <exception cref="InvalidOperationException">Thrown if no axes are specified with <paramref name="axisSet"/> or invalid names are specified</exception>
		public Cube( IAxisSet axisSet )
		{
			if( axisSet == null )
				throw new ArgumentNullException( nameof( axisSet ) );

			if( axisSet.Count == 0 )
				throw new ArgumentOutOfRangeException( nameof( axisSet ), "At least one axis must be specified" );

			if( axisSet.Any( axis => axis.Name == null ) )
				throw new ArgumentException( "Axes with null names are not supported" );

			this.AxisSet = axisSet;
			this._dictionary = new Dictionary.HybridDictionary<TValueType>( axisSet.ToArray() );
			this._errorSet = new ErrorSet( _valueMapper, axisSet.Select( axis => axis.Name ).ToArray() );
		}

		#endregion

		#region Cube Writing code

		/// <summary>
		/// Adds the given object to this cube
		/// </summary>
		/// <remarks>This method is not threadsafe</remarks>
		/// <param name="keys">The set of keys to add</param>
		/// <param name="cubeValue">The value to be added</param>
		/// <returns>True if the item was added for the first time, false if the item was combined with an existing value</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="cubeValue"/> or <paramref name="keys"/> is null</exception>
		/// <exception cref="InvalidOperationException">Thrown if a value for a mandatory axis isn't specified or the values are not valid for the given axis. Note that additional values may be specified without error</exception>
		public bool AddItem( IDictionary<string, object> keys, TValueType cubeValue )
		{
			if( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			if( cubeValue == null )
				throw new ArgumentNullException( nameof( cubeValue ) );

			var keyArray = new object [ this.AxisSet.Count ];
			for( int idx = 0; idx < keyArray.Length; idx++ )
			{
				string axisName = this.AxisSet [ idx ].Name;

				if( !keys.TryGetValue( axisName, out var keyValue ) )
					throw new InvalidOperationException( $"No key specified for mandatory axis '{axisName}' (#{idx})" );

				keyArray [ idx ] = keyValue;
			}

			return this.AddItem( keyArray, cubeValue );
		}

		/// <summary>
		/// Adds the given object to this cube
		/// </summary>
		/// <remarks>This method is not threadsafe</remarks>
		/// <param name="keys">The set of keys to add</param>
		/// <param name="cubeValue">The value to be added</param>
		/// <returns>True if the item was added for the first time, false if the item was combined with an existing value</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="keys"/> or <paramref name="cubeValue"/> is null</exception>
		/// <exception cref="InvalidOperationException">Thrown if either an invalid number of keys are specified or the specified keys are of the wrong type</exception>
		public bool AddItem( object [] keys, TValueType cubeValue )
		{
			if( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			if( cubeValue == null )
				throw new ArgumentNullException( nameof( cubeValue ) );

			if( keys.Length != this.AxisSet.Count )
				throw new InvalidOperationException( $"An invalid number of keys specified, expected {this.AxisSet.Count}, got {keys.Length}" );

			var keyArray = new uint [ this.AxisSet.Count ];
			for( int idx = 0; idx < keyArray.Length; idx++ )
			{
				if( keys [ idx ] == null )
				{
					if( this.AxisSet [ idx ].DataType.IsValueType )
						throw new InvalidOperationException( $"Null value supplied for axis #{idx}, but null is invalid for data type '{this.AxisSet [ idx ].DataType.FullName}'" );
				}
				else
				{
					if( !this.AxisSet [ idx ].DataType.IsInstanceOfType( keys [ idx ] ) )
						throw new InvalidOperationException( $"The supplied value for axis #{idx} is not of a valid type. Expected '{keys [ idx ].GetType().FullName}', received '{this.AxisSet [ idx ].DataType.FullName}'" );
				}

				keyArray [ idx ] = this._valueMapper.GetID( keys [ idx ] );
			}

			var retValue = _dictionary.AddItem( keyArray, cubeValue );
			return retValue;
		}

		/// <summary>
		/// Add the given error to the cube
		/// </summary>
		/// <remarks>This method is not threadsafe</remarks>
		/// <param name="errorKeys">The set of error keys</param>
		/// <param name="errorMessage">The error message</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="errorKeys"/> is null</exception>
		public void AddError( IDictionary<string, object> errorKeys, string errorMessage )
		{
			if( errorKeys == null )
				throw new ArgumentNullException( nameof( errorKeys ) );

			foreach( var errorKey in errorKeys )
			{
				string axisName = errorKey.Key;
				object errorValue = errorKey.Value;

				var axis = this.AxisSet [ axisName ];

				if( errorValue == null )
				{
					if( axis.DataType.IsValueType )
						throw new InvalidOperationException( $"Null value supplied for axis '{axis.Name}', but null is invalid for data type '{axis.DataType.FullName}'" );
				}
				else
				{
					if( !axis.DataType.IsInstanceOfType( errorValue ) )
						throw new InvalidOperationException( $"The supplied value for axis '{axis.Name}' is not of a valid type. Expected '{axis.DataType.FullName}', received '{errorValue.GetType().FullName}'" );
				}
			}

			this._errorSet.AddError( errorKeys, errorMessage );
		}

		#endregion

		#region ICube Members

		public IEnumerator<ICubePoint<TValueType>> GetEnumerator()
		{
			foreach( var rawKVP in _dictionary )
			{
				yield return new CubePoint<TValueType>( _valueMapper, rawKVP );
			}
		}

		public IErrorSet ErrorSet
		{
			get
			{
				return _errorSet;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool TryGetValue( object [] keys, out TValueType cubeValue )
		{
			if( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			if( keys.Length != this.AxisSet.Count )
				throw new ArgumentException( $"Expected {this.AxisSet.Count} key(s), got given {keys.Length}" );

			uint [] mappedKeys = new uint [ keys.Length ];
			for( int idx = 0; idx < mappedKeys.Length; idx++ )
			{
				if( !_valueMapper.TryGetID( keys [ idx ], out var mappedKey ) )
				{
					cubeValue = default( TValueType );
					return false;
				}

				mappedKeys [ idx ] = mappedKey;
			}

			if( this._dictionary.TryGetValue( mappedKeys, out cubeValue ) )
				return true;

			return false;
		}

		#endregion
	}
}
