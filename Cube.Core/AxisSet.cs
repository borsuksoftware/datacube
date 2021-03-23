using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard implementation of <see cref="IAxisSet"/>
	/// </summary>
	public sealed class AxisSet : IAxisSet
	{
		#region Member variables

		private readonly IAxis [] _axes;

		private readonly IDictionary<string, int> _axesByName = new Dictionary<string, int>();

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of the axis set
		/// </summary>
		/// <param name="axes">The set of axes</param>
		/// <exception cref="ArgumentNullException">Thrown if null is passed for <paramref name="axes"/></exception>
		/// <exception cref="InvalidOperationException">Thrown if multiple axes with the same name are specified</exception>
		public AxisSet( params IAxis [] axes )
		{
			if( axes == null )
				throw new ArgumentNullException( nameof( axes ) );

			_axes = new IAxis [ axes.Length ];
			for( int i = 0; i < axes.Length; i++ )
			{
				_axes [ i ] = axes [ i ];

				if( _axesByName.ContainsKey( axes [ i ].Name ) )
					throw new InvalidOperationException( $"Duplicate axes with name '{axes [ i ].Name}' found" );

				_axesByName [ axes [ i ].Name ] = i;
			}
		}

		#endregion

		#region IAxisSet Members

		public IAxis this [ int axisIndex ]
		{
			get
			{
				if( axisIndex < 0 ||
					axisIndex >= this.Count )
					throw new ArgumentOutOfRangeException(
						nameof( axisIndex ),
						$"A valid integer between 0 and {this.Count} must be specified" );

				return this._axes [ axisIndex ];
			}
		}

		public IAxis this [ string axisName ]
		{
			get
			{
				int idx = this.GetIndexOf( axisName );
				if( idx == -1 )
					throw new ArgumentException( $"No axis by name '{axisName}' found" );

				return this._axes [ idx ];
			}
		}

		public int Count
		{
			get
			{
				return this._axes.Length;
			}
		}

		public IEnumerator<IAxis> GetEnumerator()
		{
			return this._axes.Cast<IAxis>().GetEnumerator();
		}

		public int GetIndexOf( string axisName )
		{
			if( this._axesByName.TryGetValue( axisName, out var idx ) )
				return idx;

			return -1;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}
