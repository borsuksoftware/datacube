using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube.Dictionary
{
	/// <summary>
	/// Summary description for HybridErrorsDictionaryTests
	/// </summary>
	[TestClass]
	public class HybridErrorsDictionaryTests
	{
		#region Enumeration Tests

		[TestMethod]
		public void StandardEnumerator()
		{
			var axisNames = new string [] { "Axis 1", "Axis 2", "Axis 3" };
			var dictionary = new HybridErrorsDictionary( axisNames );

			var allErrors = new Dictionary<IDictionary<string, uint>, string>( new DictionaryEqualityComparer() );

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 8;
				keys [ "Axis 2" ] = 3;
				keys [ "Axis 3" ] = 4;

				allErrors.Add( keys, "(8,3,4)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 5;
				keys [ "Axis 3" ] = 4;
				allErrors.Add( keys, "(5,-,4)" );
			}


			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 3" ] = 4;
				allErrors.Add( keys, "(-,-,4)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 2;
				allErrors.Add( keys, "(2,-,-)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 2;
				keys [ "Axis 2" ] = 7;
				allErrors.Add( keys, "(2,7,-)" );
			}

			foreach( var error in allErrors )
				dictionary.AddItem( error.Key, error.Value );

			var enumerator = dictionary.GetEnumerator( null );
			while( enumerator.MoveNext() )
			{
				var entry = enumerator.Current;

				var keys = new Dictionary<string, uint>();
				for( int i = 0 ; i < entry.Key.Length ; i++ )
					if( entry.Key [ i ].HasValue )
						keys [ axisNames [ i ] ] = entry.Key [ i ].Value;

				string expectedErrorMessage;
				Assert.IsTrue( allErrors.TryGetValue( keys, out expectedErrorMessage ) );
				Assert.AreEqual( expectedErrorMessage, entry.Value );

				allErrors.Remove( keys );
			}

			Assert.AreEqual( 0,
				allErrors.Count,
				string.Format( "Missing error messages - '{0}'", string.Join( "', '", allErrors.Select( pair => pair.Value ) ) ) );
		}

		[TestMethod]
		public void FilteredEnumerator()
		{
			var axisNames = new string [] { "Axis 1", "Axis 2", "Axis 3" };
			var dictionary = new HybridErrorsDictionary( axisNames );

			var allErrors = new Dictionary<IDictionary<string, uint>, string>( new DictionaryEqualityComparer() );

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 8;
				keys [ "Axis 2" ] = 3;
				keys [ "Axis 3" ] = 4;

				allErrors.Add( keys, "(8,3,4)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 5;
				keys [ "Axis 3" ] = 4;
				allErrors.Add( keys, "(5,-,4)" );
			}


			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 3" ] = 4;
				allErrors.Add( keys, "(-,-,4)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 2;
				allErrors.Add( keys, "(2,-,-)" );
			}

			{
				var keys = new Dictionary<string, uint>();
				keys [ "Axis 1" ] = 2;
				keys [ "Axis 2" ] = 7;
				allErrors.Add( keys, "(2,7,-)" );
			}

			foreach( var error in allErrors )
				dictionary.AddItem( error.Key, error.Value );


			var filter = new Dictionary<string, uint?>();
			filter [ "Axis 2" ] = 3;

			var filteredErrors = new Dictionary<IDictionary<string, uint>, string>( new DictionaryEqualityComparer() );
			foreach( var error in allErrors )
			{
				bool include = true;
				foreach( var filterPair  in filter )
				{
					if( error.Key.ContainsKey( filterPair.Key) &&
						error.Key [ filterPair.Key ] != filterPair.Value )
					{
						include = false;
						break;
					}
				}

				if( include )
					filteredErrors.Add( error.Key, error.Value );
			}

			var enumerator = dictionary.GetEnumerator( filter );
			while( enumerator.MoveNext() )
			{
				var entry = enumerator.Current;
				var keys = new Dictionary<string, uint>();
				for( int i = 0 ; i < entry.Key.Length ; i++ )
					if( entry.Key [ i ].HasValue )
						keys [ axisNames [ i ] ] = entry.Key [ i ].Value;

				string expectedErrorMessage;
				Assert.IsTrue( filteredErrors.TryGetValue( keys, out expectedErrorMessage ) );
				Assert.AreEqual( expectedErrorMessage, entry.Value );

				filteredErrors.Remove( keys );
			}

			Assert.AreEqual( 0,
				filteredErrors.Count,
				string.Format( "Missing error messages - '{0}'", string.Join( "', '", filteredErrors.Select( pair => pair.Value ) ) ) );
		}

		#endregion

		#region DictionaryEqualityComparer Helper class

		private class DictionaryEqualityComparer : IEqualityComparer<IDictionary<string, uint>>
		{
			public bool Equals( IDictionary<string, uint> x, IDictionary<string, uint> y )
			{
				if( x.Count != y.Count )
					return false;

				foreach( var pair in x )
				{
					uint yValue;
					if( !y.TryGetValue( pair.Key, out yValue ) )
						return false;

					if( pair.Value != yValue )
						return false;
				}

				return true;
			}

			public int GetHashCode( IDictionary<string, uint> obj )
			{
				int hashCode = 0;
				foreach( var pair in obj )
					hashCode ^= pair.Key.GetHashCode() ^ pair.Value.GetHashCode();

				return hashCode;
			}
		}

		#endregion
	}
}
