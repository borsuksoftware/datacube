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

		private IAxis [] _axes;

		private IDictionary<string, int> _axesByName = new Dictionary<string, int>();

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
			for( int i = 0 ; i < axes.Length ; i++ )
			{
				_axes [ i ] = axes [ i ];

				if( _axesByName.ContainsKey( axes [ i ].Name ) )
					throw new InvalidOperationException( string.Format( "Duplicate axes with name '{0}' found", axes [ i ].Name ) );

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
						string.Format( 
							"A valid integer between 0 and {0} must be specified", 
							this.Count ) );

				return this._axes [ axisIndex ];
			}
		}

		public IAxis this [ string axisName ]
		{
			get
			{
				int idx = this.GetIndexOf( axisName );
				if( idx == -1 )
					throw new ArgumentException( string.Format( "No axis by name '{0}' found", axisName ) );

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
			int idx;
			if( this._axesByName.TryGetValue( axisName, out idx ) )
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
