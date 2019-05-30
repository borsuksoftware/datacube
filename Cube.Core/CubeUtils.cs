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
			IReadOnlyList<string> axes = null) where TValueType : ICubeValue
		{
			if( cubes == null )
				throw new ArgumentNullException( nameof( cubes ) );

			var filteredCubes = cubes.Where( cube => cube != null );
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

			IAxis[] axisList;
			if (axes != null)
			{
				var commonAxesAsDictionary = commonAxes.ToDictionary((a) => a.Name, a => a);
				var axesAsHashSet = new HashSet<string>(axes);
				commonAxes = new HashSet<IAxis>(commonAxes.Where(a => axesAsHashSet.Contains(a.Name)));

				foreach (var axisName in axes)
				{
					if (!commonAxesAsDictionary.TryGetValue(axisName, out var axis))
						throw new InvalidOperationException($"No axis '{axisName}' found on input cubes");
				}

				axisList = axes.Select( a => commonAxesAsDictionary[a]).ToArray();
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
				for( int idx = 0; idx<axisIndices.Length;idx++ )
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
