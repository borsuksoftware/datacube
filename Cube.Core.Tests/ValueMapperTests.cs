using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Summary description for ValueMapperTests
	/// </summary>
	[TestClass]
	public class ValueMapperTests
	{
		[TestMethod]
		public void NullHandling()
		{
			var valueMapper = new ValueMapper();

			var id = valueMapper.GetID( null );

			var obj = valueMapper.GetObject( id );

			Assert.IsNull( obj );
		}

		[TestMethod]
		public void StandardObject()
		{
			var valueMapper = new ValueMapper();

			var myObj = new object();
			var id = valueMapper.GetID( myObj );
			var myObj2 = valueMapper.GetObject( id );
			Assert.AreSame( myObj, myObj2 );
		}

		[TestMethod]
		public void StandardObjectAndNull()
		{
			var valueMapper = new ValueMapper();

			var myObj = new object();
			var id = valueMapper.GetID( myObj );
			var nullID = valueMapper.GetID( null );
			var myObj2 = valueMapper.GetObject( id );
			Assert.AreSame( myObj, myObj2 );

			Assert.AreNotEqual( nullID, id );
		}

		[TestMethod]
		public void ProcessLargeNumbers()
		{
			var valueMapper = new ValueMapper();
			var dict = new Dictionary<string, uint>();
			for( int i = 0 ; i < 2000000 ; i++ )
			{
				string s = i.ToString();
				dict [ s ] = valueMapper.GetID( s );
			}

			foreach( var kvp in dict )
			{
				var returnedID = valueMapper.GetID( kvp.Key );

				Assert.AreEqual( kvp.Value, returnedID );
				Assert.AreEqual( kvp.Key, valueMapper.GetObject( returnedID ) );
			}
		}
	}
}
