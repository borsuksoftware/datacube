using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Class containing utils for interacting with cubes
	/// </summary>
	public static class CubeUtils
	{
		/// <summary>
		/// Check whether or not the supplied set of objects are all valid cubes, optionally allowing for nulls
		/// </summary>
		/// <remarks>
		/// In the case whereby the supplied objects implement the appropriate cube interfaces for multiple value types then
		/// this method will return false</remarks>
		/// <param name="objects">The set of objects to inspect</param>
		/// <param name="ignoreNulls">If true, the presence of a null in the <paramref name="objects"/> collection will be ignored</param>
		/// <param name="cubeType">If all of the values in <paramref name="objects"/> are valid cubes, then the cube value type will be returned otherwise null</param>
		/// <returns></returns>
		public static bool AreObjectsCubes( IEnumerable<object> objects, bool ignoreNulls, out Type cubeType )
		{
			if( objects == null )
				throw new ArgumentNullException( nameof( objects ) );

			List<Type> possibleTypes = null;
			foreach( var @object in objects )
			{
				if( @object == null )
				{
					if( !ignoreNulls )
					{
						cubeType = null;
						return false;
					}

					continue;
				}
				var allPossibleCubeValueTypeInterfaces = @object.GetType().
					GetInterfaces().
					Where( interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof( ICube<> ) ).
					Select( t => t.GetGenericArguments() [ 0 ] ).
					ToList();

				if( allPossibleCubeValueTypeInterfaces.Count == 0 )
				{
					cubeType = null;
					return false;
				}

				if( possibleTypes == null )
				{
					possibleTypes = allPossibleCubeValueTypeInterfaces;
				}
				else
				{
					possibleTypes = possibleTypes.Union( allPossibleCubeValueTypeInterfaces ).ToList();
					if( possibleTypes.Count == 0 )
					{
						cubeType = null;
						return false;
					}
				}
			}

			if( possibleTypes != null && possibleTypes.Count == 1 )
			{
				cubeType = possibleTypes [ 0 ];
				return true;
			}

			cubeType = null;
			return false;
		}

		/// <summary>
		/// Attempt to combine the supplied set of objects as a cube if possible
		/// </summary>
		/// <param name="objects">The set of objects to combine (may contain nulls)</param>
		/// <param name="combinedCube">The combined cube if appropriate</param>
		/// <param name="combinedCubeCubeValueType">The combined cube's value type if appropriate</param>
		/// <returns>True if the objects could be combined, otherwise false</returns>
		public static bool TryCombineObjectsAsCubes( IEnumerable<object> objects, out object combinedCube, out Type combinedCubeCubeValueType )
		{
			if( objects == null )
				throw new ArgumentNullException( nameof( objects ) );

			var objectsAsList = objects.Where( o => o != null ).ToList();
			if( !CubeUtils.AreObjectsCubes( objectsAsList, true, out combinedCubeCubeValueType ) )
			{
				combinedCube = null;
				combinedCubeCubeValueType = null;
				return false;
			}

			var convertTypeMethod = typeof( System.Linq.Enumerable ).GetMethod( nameof( System.Linq.Enumerable.Cast ), new Type [] { typeof( System.Collections.IEnumerable ) } );

			var genericCubeType = typeof( ICube<> ).MakeGenericType( combinedCubeCubeValueType );
			var castGenericMethod = convertTypeMethod.MakeGenericMethod( genericCubeType );

			var objectsAsConvertedEnumerable = castGenericMethod.Invoke( null, new object [] { objectsAsList } );

			// At this point, we know that they're all of the same type, but it's still possible that they
			// couldn't be combined due to various reasons, so be safe...
			var canCombineCubesMethod = typeof( CubeUtils ).GetMethod( nameof( CanCombineCubes ) );
			var canCombineCubesMethodGeneric = canCombineCubesMethod.MakeGenericMethod( combinedCubeCubeValueType );
			var canCombine = (bool) canCombineCubesMethodGeneric.Invoke( null, new object [] { objectsAsConvertedEnumerable } );
			if( !canCombine)
			{
				combinedCube = null;
				combinedCubeCubeValueType = null;
				return false;
			}

			// at this point, there's no reason why we can't combine them, so time to do so
			var combineCubesMethod = typeof( CubeUtils ).GetMethod( nameof( CombineCubes ) );
			var combineCubesMethodGeneric = combineCubesMethod.MakeGenericMethod( combinedCubeCubeValueType );

			combinedCube = combineCubesMethodGeneric.Invoke( null, new object [] { objectsAsConvertedEnumerable, null } );
			return true;
		}

		/// <summary>
		/// Check to see if it would be possible to combine the specified cubes
		/// </summary>
		/// <typeparam name="TValueType">The value type</typeparam>
		/// <param name="cubes">The set of cubes</param>
		/// <returns>True if they could be combined, otherwise false</returns>
		public static bool CanCombineCubes<TValueType>(
			IEnumerable<ICube<TValueType>> cubes ) where TValueType: ICubeValue
		{
			if( cubes == null )
				throw new ArgumentNullException( nameof( cubes ) );

			HashSet<IAxis> commonAxes = null;
			foreach( var cube in cubes )
			{
				if( cube == null )
					continue;

				if( commonAxes == null )
				{
					commonAxes = new HashSet<IAxis>( cube.AxisSet );
				}
				else
				{
					commonAxes = new HashSet<IAxis>( commonAxes.Intersect( cube.AxisSet ) );
				}
			}

			return commonAxes != null && commonAxes.Any();
		}

		/// <summary>
		/// Combine the specified cubes over the common axes
		/// </summary>
		/// <param name="cubes">The set of cubes</param>
		/// <param name="axes">The set of axes to combine upon</param>
		/// <typeparam name="TValueType"></typeparam>
		/// <returns>A new instance of <see cref="ICube{TValueType}"/> containing all of the values from the input cubes</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="cubes"/> is null</exception>
		/// <exception cref="InvalidOperationException">Thrown if there are no common axes between the cubes</exception>
		public static ICube<TValueType> CombineCubes<TValueType>(
			IEnumerable<ICube<TValueType>> cubes,
			IReadOnlyList<string> axes = null ) where TValueType : ICubeValue
		{
			if( cubes == null )
				throw new ArgumentNullException( nameof( cubes ) );

			var filteredCubes = cubes.Where( cube => cube != null ).ToList();
			if( !filteredCubes.Any() )
				return null;

			HashSet<IAxis> commonAxes = null;
			foreach( var cube in filteredCubes )
			{
				if( commonAxes == null )
				{
					commonAxes = new HashSet<IAxis>( cube.AxisSet );
				}
				else
				{
					commonAxes = new HashSet<IAxis>( commonAxes.Intersect( cube.AxisSet ) );
				}
			}

			if( commonAxes.Count == 0 )
				throw new InvalidOperationException( "Unable to combine cubes - zero common axes" );

			IAxis [] axisList;
			if( axes != null )
			{
				var commonAxesAsDictionary = commonAxes.ToDictionary( ( a ) => a.Name, a => a );
				var axesAsHashSet = new HashSet<string>( axes );
				commonAxes = new HashSet<IAxis>( commonAxes.Where( a => axesAsHashSet.Contains( a.Name ) ) );

				foreach( var axisName in axes )
				{
					if( !commonAxesAsDictionary.TryGetValue( axisName, out var axis ) )
						throw new InvalidOperationException( $"No axis '{axisName}' found on input cubes" );
				}

				axisList = axes.Select( a => commonAxesAsDictionary [ a ] ).ToArray();
			}
			else
			{
				axisList = commonAxes.ToArray();
			}

			var outputCube = new Cube<TValueType>( new AxisSet( axisList ) );

			var axisNames = new HashSet<string>( axisList.Select( axis => axis.Name ) );
			foreach( var cube in filteredCubes )
			{
				int [] axisIndices = new int [ axisList.Length ];
				for( int idx = 0; idx < axisIndices.Length; idx++ )
				{
					axisIndices [ idx ] = cube.AxisSet.GetIndexOf( axisList [ idx ].Name );
				}

				object [] keys = new object [ axisList.Length ];
				foreach( var point in cube )
				{
					for( int idx = 0; idx < axisIndices.Length; ++idx )
					{
						keys [ idx ] = point.GetAxisValue( axisIndices [ idx ] );
					}

					outputCube.AddItem( keys, point.CubeValue );

					foreach( var error in cube.ErrorSet )
					{
						var filteredErrorKeys = new Dictionary<string, object>();
						foreach( var axisName in error.ErrorAxes.Where( k => axisNames.Contains( k ) ) )
							filteredErrorKeys [ axisName ] = error.GetErrorKey( axisName );

						outputCube.AddError( filteredErrorKeys, error.ErrorMessage );
					}
				}
			}

			return outputCube;
		}
	}
}
