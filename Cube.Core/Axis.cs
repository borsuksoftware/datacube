using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard implementation of <see cref="IAxis"/>
	/// </summary>
	public sealed class Axis : IAxis
	{
		#region IAxis Members

		public bool AllowTotals { get; private set; }

		public Type DataType { get; private set; }

		public string Name { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of the axis
		/// </summary>
		/// <param name="name">The name of the axis</param>
		/// <param name="dataType">The type of the axis</param>
		/// <param name="allowTotals">Whether or not to allow totals</param>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="name"/> or <paramref name="dataType"/> is null</exception>
		public Axis( string name, Type dataType, bool allowTotals = true )
		{
			if( string.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( nameof( name ) );

			if( dataType == null )
				throw new ArgumentNullException( nameof( dataType ) );

			this.AllowTotals = allowTotals;
			this.DataType = dataType;
			this.Name = name;
		}

		#endregion

		public override bool Equals( object obj )
		{
			var otherObj = obj as IAxis;
			if( otherObj == null )
				return false;

			if( otherObj.AllowTotals != this.AllowTotals ||
				otherObj.DataType != this.DataType ||
				otherObj.Name != this.Name )
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = this.AllowTotals.GetHashCode() ^
				this.DataType.GetHashCode() ^
				this.Name.GetHashCode();

			return hashCode;
		}
	}
}
