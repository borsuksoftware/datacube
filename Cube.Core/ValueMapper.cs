using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Class which can map between values and a redirection number
	/// </summary>
	internal class ValueMapper
	{
		#region Member variables

		private IDictionary<object, uint> _objectToIDMap = new Dictionary<object, uint>();

		private object [] [] [] _objectsByID;

		uint _nextValue = 1;

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates an empty instance of the <see cref="ValueMapper"/>
		/// </summary>
		public ValueMapper()
		{
			_objectsByID = new object [ 32 ] [] [];
		} 

		#endregion

		#region Class Functionality

		/// <summary>
		/// Specialised method to allow the querying of a <see cref="ValueMapper"/> to see
		/// whether it has an entry for the supplied value.
		/// </summary>
		/// <remarks>This method is inherently threadsafe</remarks>
		/// <param name="object">The object</param>
		/// <param name="id">The ID for this object if available</param>
		/// <returns>True if the object was already in this set, otherwise false</returns>
		public bool TryGetID( object @object, out uint id )
		{
			if( @object == null )
			{
				id = 0;
				return true;
			}

			return _objectToIDMap.TryGetValue( @object, out id );
		}

		/// <summary>
		/// Gets an ID, caching the object if necessary, for the supplied object
		/// </summary>
		/// <remarks>This method cannot be used in a multi-threaded environment without external synchronisation</remarks>
		/// <param name="object">The object to get an ID for</param>
		/// <returns>The ID to use</returns>
		public uint GetID( object @object )
		{
			if( @object == null )
				return 0;

			uint id;
			if( _objectToIDMap.TryGetValue( @object, out id ) )
				return id;

			id = _nextValue;
			_nextValue++;
			_objectToIDMap [ @object ] = id;
			uint highLevelKey = id / ( 1024 * 1024 );
			uint midLevelKey = ( id / 1024 ) & 1023;
			uint lowLevelKey = id & 1023;

			var midLevelSet = this._objectsByID [ highLevelKey ];
			if( midLevelSet == null )
			{
				midLevelSet = new object [ 1024 ] [];
				_objectsByID [ highLevelKey ] = midLevelSet;
			}

			var lowLevelSet = midLevelSet [ midLevelKey ];
			if( lowLevelSet == null )
			{
				lowLevelSet = new object [ 1024 ];
				midLevelSet [ midLevelKey ] = lowLevelSet;
			}

			lowLevelSet [ lowLevelKey ] = @object;
			return id;
		}

		/// <summary>
		/// Gets the object represented by the specified ID
		/// </summary>
		/// <param name="id">The ID</param>
		/// <returns>The cached object</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid ID is specified</exception>
		public object GetObject( uint id )
		{
			if( id == 0 )
				return null;

			if (id >= _nextValue)
				throw new ArgumentOutOfRangeException(nameof(id),
					$"A valid value between 0 and {_nextValue} (exclusive) must be specified");

			uint highLevelKey = id / ( 1024 * 1024 );
			uint midLevelKey = ( id / 1024 ) & 1023;
			uint lowLevelKey = id & 1023;

			var midLevelSet = _objectsByID [ highLevelKey ];
			var lowLevelSet = midLevelSet [ midLevelKey ];
			var obj = lowLevelSet [ lowLevelKey ];
			return obj;
		}

		#endregion
	}
}
