using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Summary description for AxisSetTests
	/// </summary>
	[TestClass]
	public class AxisSetTests
	{

		[TestMethod]
		public void HandleDuplicateAxisNames()
		{
			try
			{
				var axisSet = new AxisSet(
					new Axis( "Axis", typeof( string ), true ),
					new Axis( "Axis", typeof( string ), false ) );

				Assert.Fail( "Duplicate axis allowed" );
			}
			catch( InvalidOperationException )
			{

			}
		}

		[TestMethod]
		public void GetEnumerator_Validate()
		{
			var items = new Tuple<string, Type, bool> []
			{
				Tuple.Create( "Axis1", typeof( string ), true ),
				Tuple.Create( "Axis2", typeof( double ), true ),
				Tuple.Create( "Axis3", typeof( float ), true ),
			};

			var axisSet = new AxisSet( items.Select( tuple => new Axis( tuple.Item1, tuple.Item2, tuple.Item3 ) ).ToArray() );

			var enumerator1 = items.GetEnumerator();
			var enumerator2 = axisSet.GetEnumerator();

			while( enumerator1.MoveNext() )
			{
				Assert.IsTrue( enumerator2.MoveNext() );

				var current = (Tuple<string, Type, bool>) enumerator1.Current;

				Assert.AreEqual( current.Item1, enumerator2.Current.Name );
				Assert.AreEqual( current.Item2, enumerator2.Current.DataType );
				Assert.AreEqual( current.Item3, enumerator2.Current.AllowTotals );
			}

			Assert.IsFalse( enumerator2.MoveNext() );
		}
	}
}
