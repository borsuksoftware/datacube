using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Summary description for CubeTests
	/// </summary>
	[TestClass]
	public class CubeTests
	{
		#region Negative Tests

		[TestMethod]
		public void AddIncorrectNumberOfKeys()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			try
			{
				cube.AddItem( new object [] { "Value1" }, new CubeValueDouble( 2 ) );
				Assert.Fail( "Should have thrown" );
			}
			catch
			{
			}

			try
			{
				cube.AddItem( new object [] { "Value1", DateTime.Today, 2.3 }, new CubeValueDouble( 2 ) );
				Assert.Fail( "Should have thrown" );
			}
			catch
			{
			}
		}

		[TestMethod]
		public void AddIncorrectDataType()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			try
			{
				cube.AddItem( new object [] { DateTime.Today, "Value1" }, new CubeValueDouble( 2 ) );
				Assert.Fail( "Should have thrown" );
			}
			catch
			{
			}
		}

		#endregion

		#region Error Handling

		[TestMethod]
		public void AddRelevantError()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			var errorKeys = new Dictionary<string, object>();
			errorKeys [ "Axis 1" ] = "Bob";
			cube.AddError( errorKeys, "Error" );
		}

		[TestMethod]
		public void AddErrorWithWrongDataType()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			var errorKeys = new Dictionary<string, object>();
			errorKeys [ "Axis 1 " ] = DateTime.Today;

			try
			{
				cube.AddError( errorKeys, "Error" );
				Assert.Fail( "Shouldn't be able to add error" );
			}
			catch
			{
			}
		}

		[TestMethod]
		public void AddErrorWithNonExistentKey()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			var errorKeys = new Dictionary<string, object>();
			errorKeys [ "Axis 2" ] = DateTime.Today;

			try
			{
				cube.AddError( errorKeys, "Error" );
				Assert.Fail( "Shouldn't be able to add error" );
			}
			catch
			{
			}
		}

		#endregion

		[TestMethod]
		[Description( "Covers checking that when a value is added with an existing set of keys, they're combined" )]
		public void AddOverridingValues()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Axis 2", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );

			var refDate = DateTime.Today;
			var keys = new object [] { "Val1", "Val2", refDate };
			var keys2 = new object [] { "Val1", "Val2", refDate };

			var val1 = new CubeValueDouble( 2.3 );
			var val2 = new CubeValueDouble( 3.7 );

			Assert.IsTrue( cube.AddItem( keys, val1 ) );
			Assert.IsFalse( cube.AddItem( keys, val2 ) );

			Assert.AreEqual( 1, cube.Count() );
			var combinedValue = cube.First();

			Assert.AreNotSame( val1, combinedValue.CubeValue );
			Assert.AreNotSame( val2, combinedValue.CubeValue );
			Assert.AreEqual( 2.3 + 3.7, combinedValue.CubeValue.Val );
		}

		[TestMethod]
		[Description( "Covers the consumption of data as an interator")]
		public void Iteration()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Axis 2", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );
			var simpleDictionary = new Dictionary<DictionaryKey, double>();

			var refDate = DateTime.Today;
			for( int loopIdx1 = 0 ; loopIdx1 < 10 ; loopIdx1++ )
			{
				for( int loopIdx2 = 0 ; loopIdx2 < 10 ; loopIdx2++ )
				{
					for( int loopIdx3 = 0 ; loopIdx3 < 100 ; loopIdx3++ )
					{
						object [] keys = new object [ 3 ];
						keys [ 0 ] = string.Format( "Axis 1 - {0}", loopIdx1 );
						keys [ 1 ] = string.Format( "Axis 2 - {0}", loopIdx2 );
						keys [ 2 ] = refDate.AddDays( loopIdx3 );

						CubeValueDouble val = new CubeValueDouble();
						val.Val = loopIdx1 * 1000;
						val.Val += loopIdx2 * 100;
						val.Val += loopIdx3;
						cube.AddItem( keys, val );

						simpleDictionary [ new DictionaryKey( (string) keys [ 0 ], (string) keys [ 1 ], (DateTime) keys [ 2 ] ) ] = val.Val;
					}
				}
			}

			// Check random access
			{
				CubeValueDouble matchedValue;
				Assert.IsTrue( cube.TryGetValue( new object [] { "Axis 1 - 5", "Axis 2 - 7", refDate.AddDays( 22 ) }, out matchedValue ) );
				Assert.AreEqual( 5722, matchedValue.Val );
			}

			// Ensure that we can iterate over each point in a cube
			foreach( var cubePoint in cube )
			{
				string axis1 = (string) cubePoint.GetAxisValue( 0 );
				string axis2 = (string) cubePoint.GetAxisValue( 1 );
				DateTime date = (DateTime) cubePoint.GetAxisValue( 2 );

				var dictKey = new DictionaryKey( axis1, axis2, date );
				double expectedValue;
				if( !simpleDictionary.TryGetValue( dictKey, out expectedValue ) )
					Assert.Fail( "No value found for key" );

				simpleDictionary.Remove( dictKey );

				Assert.AreEqual( expectedValue, cubePoint.CubeValue.Val );
			}

			Assert.AreEqual( 0, simpleDictionary.Count );
		}

		[TestMethod]
		public void RandomAccess()
		{
			var axisSet = new AxisSet( new Axis []
			{
				new Axis( "Axis 1", typeof( string ), true ),
				new Axis( "Axis 2", typeof( string ), true ),
				new Axis( "Expiry Date", typeof( DateTime ), true )
			} );

			var cube = new Cube.Cube<CubeValueDouble>( axisSet );
			var simpleDictionary = new Dictionary<DictionaryKey, double>();

			var refDate = DateTime.Today;
			for( int loopIdx1 = 0 ; loopIdx1 < 10 ; loopIdx1++ )
			{
				for( int loopIdx2 = 0 ; loopIdx2 < 10 ; loopIdx2++ )
				{
					for( int loopIdx3 = 0 ; loopIdx3 < 30 ; loopIdx3++ )
					{
						object [] keys = new object [ 3 ];
						keys [ 0 ] = string.Format( "Axis 1 - {0}", loopIdx1 );
						keys [ 1 ] = string.Format( "Axis 2 - {0}", loopIdx2 );
						keys [ 2 ] = refDate.AddDays( loopIdx3 );

						CubeValueDouble val = new CubeValueDouble();
						val.Val = loopIdx1 * 1000;
						val.Val += loopIdx2 * 100;
						val.Val += loopIdx3;
						cube.AddItem( keys, val );
					}
				}
			}

			// Check random access
			{
				CubeValueDouble matchedValue;
				Assert.IsTrue( cube.TryGetValue( new object [] { "Axis 1 - 5", "Axis 2 - 7", refDate.AddDays( 22 ) }, out matchedValue ) );
				Assert.AreEqual( 5722, matchedValue.Val );
			}
		}

		#region Helper classes

		private class CubeValueDouble : ICubeValue
		{
			public double Val;

			public CubeValueDouble( double val )
			{
				this.Val = val;
			}

			public CubeValueDouble()
			{ }

			public void Add( ICubeValue cubeValue )
			{
				if( cubeValue == null )
					return;

				var other = cubeValue as CubeValueDouble;
				if( other == null )
					throw new NotSupportedException();

				this.Val += other.Val;
			}

			public ICubeValue Clone()
			{
				return new CubeValueDouble() { Val = this.Val };
			}
		}

		private class DictionaryKey
		{
			public string Axis1 { get; private set; }
			public string Axis2 { get; private set; }
			public DateTime RefDate { get; private set; }

			public DictionaryKey( string axis1, string axis2, DateTime refDate )
			{
				this.Axis1 = axis1;
				this.Axis2 = axis2;
				this.RefDate = refDate;
			}

			public override bool Equals( object obj )
			{
				var other = obj as DictionaryKey;
				if( other == null )
					return false;

				if( string.Compare( this.Axis1, other.Axis1 ) != 0 ||
					string.Compare( this.Axis2, other.Axis2 ) != 0 ||
					this.RefDate != other.RefDate )
					return false;

				return true;
			}

			public override int GetHashCode()
			{
				int hashCode = 0;

				hashCode ^= this.Axis1.GetHashCode();
				hashCode ^= this.Axis2.GetHashCode();
				hashCode ^= this.RefDate.GetHashCode();

				return hashCode;
			}
		}

		#endregion
	}
}
