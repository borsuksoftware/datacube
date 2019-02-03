using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube.Dictionary
{
	internal sealed class HybridDictionary<TValueType> where TValueType : ICubeValue
	{
		#region Member variables

		IDictionary<uint, object> _rootAxes = new Dictionary<uint, object>();

		IAxis [] _axes;

		#endregion

		public HybridDictionary( IAxis [] axes )
		{
			if( axes == null )
				throw new ArgumentNullException( nameof( axes ) );

			_axes = axes;
			switch( axes.Length )
			{
				case 0:
					throw new NotSupportedException( "At least 1 axis must be specified" );
			}
		}

		public bool TryGetValue( uint [] keys, out TValueType @value )
		{
			var node = _rootAxes;
			for( int idx = 0 ; idx < keys.Length - 1 ; idx++ )
			{
				var key = keys [ idx ];

				object intermediateNodeObj;
				if( !node.TryGetValue( key, out intermediateNodeObj ) )
				{
					@value = default( TValueType );
					return false;
				}

				var intermediateNode = intermediateNodeObj as IDictionary<uint, object>;
				if( intermediateNode == null )
					throw new InvalidOperationException( "Invalid intermediate node found in hybrid structure" );

				node = intermediateNode;
			}

			// At this point, we're guaranteed to have an IDictionary<object,object> in node, but where the end value is of type T
			object valueObj;
			if( !node.TryGetValue( keys [ keys.Length - 1 ], out valueObj ) )
			{
				value = default( TValueType );
				return false;
			}

			@value = (TValueType) valueObj;
			return true;
		}

		/// <summary>
		/// Adds the supplied item to the set, cloning as appropriate
		/// </summary>
		/// <param name="keys">The set of keys to be added for</param>
		/// <param name="value">The value to be added</param>
		/// <returns>True if the item was new, false if there was an existing item with the set of keys</returns>
		public bool AddItem( uint [] keys, TValueType value )
		{
			var dictionary = this._rootAxes;
			for( int idx = 0 ; idx < keys.Length ; idx++ )
			{
				if( idx == keys.Length - 1 )
				{
					object obj;
					if( !dictionary.TryGetValue( keys [ idx ], out obj ) )
					{
						obj = value.Clone();
						dictionary [ keys [ idx ] ] = obj;
						return true;
					}
					else
					{
						TValueType existingObj = (TValueType) obj;
						existingObj.Add( value );
						return false;
					}
				}
				else
				{
					object obj;
					if( !dictionary.TryGetValue( keys [ idx ], out obj ) )
					{
						obj = new Dictionary<uint, object>();
						dictionary [ keys [ idx ] ] = obj;
					}

					dictionary = obj as IDictionary<uint, object>;
					if( dictionary == null )
						throw new InvalidOperationException( "Invalid intermediate node" );
				}
			}

			throw new InvalidOperationException( "Tried to add to a zero axed cube" );
		}

		public IEnumerator<KeyValuePair<uint[],TValueType>> GetEnumerator()
		{
			var enumerators = new IEnumerator<KeyValuePair<uint,object>> [ _axes.Length ];
			var rootDictionary = _rootAxes;
			for( int i = 0 ; i < this._axes.Length ; i++ )
			{
				if( rootDictionary == null )
					throw new InvalidOperationException();

				enumerators [ i ] = rootDictionary.GetEnumerator();
				if( !enumerators [ i ].MoveNext() )
					yield break;

				rootDictionary = enumerators [ i ].Current.Value as IDictionary<uint, object>;
			}

			// We now have fully populated root enumerators..
			while( true )
			{
				{
					uint [] keyArray = new uint [ enumerators.Length ];
					for( int i = 0 ; i < keyArray.Length ; i++ )
						keyArray [ i ] = enumerators [ i ].Current.Key;

					TValueType cubeValue = (TValueType) enumerators [ enumerators.Length - 1 ].Current.Value;

					yield return new KeyValuePair<uint [], TValueType>( keyArray, cubeValue );
				}

				bool found = false;
				for( int idx = enumerators.Length - 1 ; idx >= 0 ; --idx )
				{
					if( enumerators [ idx ].MoveNext() )
					{
						// At this stage, we have more items in our collection..
						rootDictionary = enumerators [ idx ].Current.Value as IDictionary<uint, object>;
						for( int innerLoop = idx + 1 ; innerLoop < enumerators.Length ; innerLoop++ )
						{
							enumerators [ innerLoop ] = rootDictionary.GetEnumerator();
							if( !enumerators [ innerLoop ].MoveNext() )
								throw new InvalidOperationException( "Invalid intermediate state" );

							rootDictionary = enumerators [ innerLoop ].Current.Value as IDictionary<uint,object>;
						}
						found = true;
						break;
					}
				}

				if( !found )
					yield break;
			}
		}
	}
}
