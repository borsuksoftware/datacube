using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube.Dictionary
{
	/// <summary>
	/// Hybrid dictionary for representing errors as a sparse tree structure
	/// </summary>
	/// <remarks>This is similar to the standard <see cref="HybridDictionary{TValueType}"/> class but with support for not requiring
	/// each axis to be specified</remarks>
	internal class HybridErrorsDictionary
	{
		#region Member variables

		string [] _axisNames;

		NodeType _rootNode = new NodeType();

		#endregion

		#region Construction logic

		/// <summary>
		/// Creates a new instance of the <see cref="HybridErrorsDictionary"/> class
		/// </summary>
		/// <param name="axisNames">The set of axis names</param>
		public HybridErrorsDictionary( string [] axisNames )
		{
			if( axisNames == null )
				throw new ArgumentNullException( nameof( axisNames ) );

			if( axisNames.Any( name => name == null ) )
				throw new ArgumentException( "Found an axis with null name, this is not supported", nameof( axisNames ) );

			_axisNames = axisNames;
			switch( axisNames.Length )
			{
				case 0:
					throw new NotSupportedException( "At least 1 axis must be specified" );
			}
		}

		#endregion

		#region Writing Code

		/// <summary>
		/// Adds the given error to the set of items for the specified set of keys
		/// </summary>
		/// <param name="keys">The set of keys</param>
		/// <param name="errorMessage">The error message to be added</param>
		public void AddItem( IDictionary<string, uint> keys, string errorMessage )
		{
			if( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			int matchedCount = 0;
			foreach( var axis in _axisNames )
			{
				if( keys.ContainsKey( axis ) )
					matchedCount++;
			}

			if( matchedCount != keys.Count )
				throw new InvalidOperationException( "Error keys contained a value for an axis not defined within the set" );


			var rootDictionary = this._rootNode;
			for( int idx = 0 ; idx < this._axisNames.Length ; idx++ )
			{
				uint errorKey;
				bool hasKey = keys.TryGetValue( _axisNames [ idx ], out errorKey );

				if( idx == _axisNames.Length - 1 )
				{
					// Final leaf node
					if( hasKey )
					{
						if( rootDictionary.ValueSpecified == null )
							rootDictionary.ValueSpecified = new Dictionary<uint, object>();

						object obj;
						if( !rootDictionary.ValueSpecified.TryGetValue( errorKey, out obj ) )
						{
							obj = new List<string>();
							rootDictionary.ValueSpecified.Add( errorKey, obj );
						}

						( (ICollection<string>) obj ).Add( errorMessage );
					}
					else
					{
						if( rootDictionary.NoValueSpecified == null )
							rootDictionary.NoValueSpecified = new List<string>();

						( (ICollection<string>) rootDictionary.NoValueSpecified ).Add( errorMessage );
					}
				}
				else
				{
					// Intermediate node
					if( hasKey )
					{
						if( rootDictionary.ValueSpecified == null )
							rootDictionary.ValueSpecified = new Dictionary<uint, object>();


						object obj;
						if( !rootDictionary.ValueSpecified.TryGetValue( errorKey, out obj ) )
						{
							obj = new NodeType();
							rootDictionary.ValueSpecified [ errorKey ] = obj;
						}

						rootDictionary = (NodeType) obj;
					}
					else
					{
						if( rootDictionary.NoValueSpecified == null )
							rootDictionary.NoValueSpecified = new NodeType();

						rootDictionary = (NodeType) rootDictionary.NoValueSpecified;
					}
				}
			}
		}

		#endregion

		#region Enumeration code

		/// <summary>
		/// Gets an enumerator over all possible error messages which match the optional filter keys
		/// </summary>
		/// <param name="filterKeys">Optional set of filter keys</param>
		/// <returns>An enumerator over the set of matching errors</returns>
		public IEnumerator<KeyValuePair<uint? [], string>> GetEnumerator( IDictionary<string, uint?> filterKeys )
		{
			// Build up a mapped value of the set of filters
			int mappedFilters = 0;
			uint? [] filterValues = new uint? [ _axisNames.Length ];
			for( int i = 0 ; i < _axisNames.Length ; i++ )
			{
				filterValues [ i ] = null;

				uint? filterValue;
				if( filterKeys != null && filterKeys.TryGetValue( _axisNames [ i ], out filterValue ) )
				{
					filterValues [ i ] = filterValue;
					mappedFilters++;
				}
			}

			if( filterKeys != null && filterKeys.Count != mappedFilters )
				throw new InvalidOperationException( "Filter key specified which isn't a valid axis" );

			// Build up the initial set of enumerators which will be processed at a high level
			var enumerators = new IEnumerator<KeyValuePair<uint?, object>> [ _axisNames.Length ];
			var rootNode = _rootNode;
			for( int i = 0 ; i < this._axisNames.Length ; i++ )
			{
				if( rootNode == null )
					throw new InvalidOperationException();

				enumerators [ i ] = rootNode.GetEnumerator( filterValues [ i ] );
				if( !enumerators [ i ].MoveNext() )
					yield break;

				rootNode = enumerators [ i ].Current.Value as NodeType;
			}

			// Iterate over the enumerators
			while( true )
			{
				// Return messages for the first point
				{
					uint? [] keys = new uint? [ _axisNames.Length ];
					for( int i = 0 ; i < keys.Length ; i++ )
						keys [ i ] = enumerators [ i ].Current.Key;

					var messages = (ICollection<string>) enumerators [ keys.Length - 1 ].Current.Value;

					foreach( var message in messages )
						yield return new KeyValuePair<uint? [], string>( keys, message );
				}

				// Update the enumerators
				bool found = false;
				for( int i = enumerators.Length - 1 ; i >= 0 ; --i )
				{
					if( enumerators [ i ].MoveNext() )
					{
						rootNode = enumerators [ i ].Current.Value as NodeType;
						for( int innerLoop = i + 1 ; innerLoop < enumerators.Length ; innerLoop++ )
						{
							enumerators [ innerLoop ] = rootNode.GetEnumerator( filterValues [ innerLoop ] );
							if( !enumerators [ innerLoop ].MoveNext() )
								throw new InvalidOperationException( "Invalid intermediate state" );

							rootNode = enumerators [ innerLoop ].Current.Value as NodeType;
						}

						found = true;
						break;
					}
				}

				if( !found )
					yield break;
			}

		}

		#endregion

		#region NodeType helper class

		/// <summary>
		/// Class representing a node type within the data structure
		/// </summary>
		private class NodeType
		{
			/// <summary>
			/// Object representing nodes which have no value specified for the given axis
			/// </summary>
			public object NoValueSpecified;

			/// <summary>
			/// Object representing nodes where there's a value for the given axis
			/// </summary>
			public IDictionary<uint, object> ValueSpecified;

			/// <summary>
			/// Helper method to get the enumerator over the various values specified
			/// </summary>
			/// <returns>The enumerator</returns>
			public IEnumerator<KeyValuePair<uint?, object>> GetEnumerator( uint? filterValue )
			{
				if( this.NoValueSpecified != null )
					yield return new KeyValuePair<uint?, object>( null, this.NoValueSpecified );

				if( this.ValueSpecified != null )
					foreach( var pair in this.ValueSpecified )
					{
						if( !filterValue.HasValue || filterValue.Value == pair.Key )
							yield return new KeyValuePair<uint?, object>( pair.Key, pair.Value );
					}
			}
		}

		#endregion
	}
}
