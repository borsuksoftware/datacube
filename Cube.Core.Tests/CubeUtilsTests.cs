using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube
{
	#region DoubleValue Class

	public class DoubleValue : ICubeValue
	{
		public double Value { get; set; }

		public DoubleValue( double value )
		{
			this.Value = value;
		}

		public ICubeValue Clone()
		{
			return new DoubleValue( this.Value );
		}

		public void Add( ICubeValue cubeValue )
		{
			var otherValue = cubeValue as DoubleValue;
			if( otherValue == null )
				throw new InvalidOperationException( $"Unable to add value of type '{cubeValue.GetType()}'" );

			this.Value += otherValue.Value;
		}
	}

	#endregion

	#region LongValue Class

	public class LongValue : ICubeValue
	{
		public long Value { get; set; }

		public LongValue( long value )
		{
			this.Value = value;
		}

		public ICubeValue Clone()
		{
			return new LongValue( this.Value );
		}

		public void Add( ICubeValue cubeValue )
		{
			var otherValue = cubeValue as LongValue;
			if( otherValue == null )
				throw new InvalidOperationException( $"Unable to add value of type '{cubeValue.GetType()}'" );

			this.Value += otherValue.Value;
		}
	}

	#endregion

	[TestClass]
	public class CubeUtilsTests
	{
		#region AreObjectsCubes Tests

		[TestClass]
		public class AreObjectsCubes
		{
			[TestMethod]
			public void EmptySet()
			{
				Assert.IsFalse( CubeUtils.AreObjectsCubes( Array.Empty<object>(), false, out var cubeType ) );
			}

			[TestMethod]
			public void NullSet()
			{
				Assert.ThrowsException<ArgumentNullException>( () => { CubeUtils.AreObjectsCubes( null, false, out var cubeType ); } );
			}

			[TestMethod]
			public void OnlyNulls()
			{
				Assert.IsFalse( CubeUtils.AreObjectsCubes( new object [] { null, null }, false, out var cubeType ) );
			}

			[TestMethod]
			public void WithNulls()
			{
				(var cube1, var cube2) = CubePoints();

				Assert.IsTrue( CubeUtils.AreObjectsCubes( new object [] { cube1, null, cube2 }, true, out var cubeType ) );
				Assert.AreEqual( typeof( DoubleValue ), cubeType );
			}

			[TestMethod]
			public void DifferentValueTypes()
			{
				var axes1 = new Axis [] { new Axis( "Axis 1", typeof( string ) ) };
				var axes2 = new Axis [] { new Axis( "Axis 1", typeof( string ) ) };

				var cube1 = new Cube<DoubleValue>( new AxisSet( axes1 ) );
				var cube2 = new Cube<LongValue>( new AxisSet( axes2 ) );

				Assert.IsFalse( CubeUtils.AreObjectsCubes( new object [] { cube1, cube2 }, true, out var cubeType ) );
			}
		}

		#endregion

		#region TryCombineObjectsAsCubes Tests

		[TestClass]
		public class TryCombineObjectsAsCubes
		{
			[TestMethod]
			public void NullArgs()
			{
				Assert.ThrowsException<ArgumentNullException>( () => CubeUtils.TryCombineObjectsAsCubes( null, out var combinedCube, out var combinedCubeCubeValueType ) );
			}

			[TestMethod]
			public void EmptyInputs()
			{
				Assert.IsFalse( CubeUtils.TryCombineObjectsAsCubes( new object [ 0 ], out var combinedCube, out var type ) );
			}

			[TestMethod]
			public void NoOverlappingAxes()
			{
				var axisSet1 = new AxisSet( new Axis( "Axis 1", typeof( string ) ) );
				var axisSet2 = new AxisSet( new Axis( "Axis 2", typeof( string ) ) );

				var cube1 = new Cube<DoubleValue>( axisSet1 );
				var cube2 = new Cube<DoubleValue>( axisSet2 );

				Assert.IsFalse( CubeUtils.TryCombineObjectsAsCubes( new object [] { cube1, cube2 }, out var cube, out var cubeValueType ) );
			}
		}

		#endregion

		#region CombineCubes Tests

		[TestClass]
		public class CombineCubes
		{

			[TestMethod]
			public void Simple()
			{
				(var cube1, var cube2) = CubePoints();

				var combinedCube = CubeUtils.CombineCubes( new [] { cube1, cube2 } );
				Assert.AreEqual( 2, combinedCube.AxisSet.Count );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str1" ) );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str3" ) );

				Assert.AreEqual( 1, combinedCube.Count() );

				var val = combinedCube.Single();
				Assert.AreEqual( 11, val.CubeValue.Value );

			}

			[TestMethod]
			public void Axes_Order1()
			{
				(var cube1, var cube2) = CubePoints();

				var combinedCube = CubeUtils.CombineCubes( new [] { cube1, cube2 }, new string [] { "str1", "str3" } );
				Assert.AreEqual( 2, combinedCube.AxisSet.Count );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str1" ) );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str3" ) );

				var axisNames = combinedCube.AxisSet.Select( a => a.Name ).ToList();
				Assert.AreEqual( "str1", axisNames [ 0 ] );
				Assert.AreEqual( "str3", axisNames [ 1 ] );

				Assert.AreEqual( 1, combinedCube.Count() );

				var val = combinedCube.Single();
				Assert.AreEqual( 11, val.CubeValue.Value );
			}

			[TestMethod]
			public void Axes_Order2()
			{
				(var cube1, var cube2) = CubePoints();

				var combinedCube = CubeUtils.CombineCubes( new [] { cube1, cube2 }, new string [] { "str3", "str1" } );
				Assert.AreEqual( 2, combinedCube.AxisSet.Count );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str1" ) );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str3" ) );

				var axisNames = combinedCube.AxisSet.Select( a => a.Name ).ToList();
				Assert.AreEqual( "str3", axisNames [ 0 ] );
				Assert.AreEqual( "str1", axisNames [ 1 ] );

				Assert.AreEqual( 1, combinedCube.Count() );

				var val = combinedCube.Single();
				Assert.AreEqual( 11, val.CubeValue.Value );
			}

			[TestMethod]
			public void Axes_Subset()
			{
				(var cube1, var cube2) = CubePoints();

				var combinedCube = CubeUtils.CombineCubes( new [] { cube1, cube2 }, new string [] { "str3" } );
				Assert.AreEqual( 1, combinedCube.AxisSet.Count );
				Assert.IsTrue( combinedCube.AxisSet.Any( axis => axis.Name == "str3" ) );

				Assert.AreEqual( 1, combinedCube.Count() );

				var val = combinedCube.Single();
				Assert.AreEqual( 11, val.CubeValue.Value );
			}
		}

		#endregion

		#region Helpers

		private static Tuple<Cube<DoubleValue>, Cube<DoubleValue>> CubePoints()
		{
			var cube1 = new Cube<DoubleValue>(
				new AxisSet( new []
				{
					new Axis("str1", typeof(string), true),
					new Axis("str2", typeof(string), true),
					new Axis("str3", typeof(string), true),
				} ) );

			var cube2 = new Cube<DoubleValue>(
				new AxisSet( new []
				{
					new Axis("str1", typeof(string), true),
					new Axis("str3", typeof(string), true),
				} ) );

			{
				var keys1 = new Dictionary<string, object>();
				keys1 [ "str1" ] = "a1";
				keys1 [ "str2" ] = "a2";
				keys1 [ "str3" ] = "a3";
				cube1.AddItem( keys1, new DoubleValue( 5 ) );
			}
			{
				var keys1 = new Dictionary<string, object>();
				keys1 [ "str1" ] = "a1";
				keys1 [ "str2" ] = "a2a";
				keys1 [ "str3" ] = "a3";
				cube1.AddItem( keys1, new DoubleValue( 6 ) );
			}

			return Tuple.Create( cube1, cube2 );
		}

		#endregion
	}
}