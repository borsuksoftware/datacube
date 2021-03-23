using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard wrapper for an error 
	/// </summary>
	/// <remarks>Note that this class is designed to be solely used by <see cref="Cube{TValueType}"/>
	/// hence it being marked as internal.</remarks>
	internal class Error : IError
	{
		#region Member variables

		private readonly ValueMapper _valueMapper;
		private readonly string [] _axisNames;
		private readonly uint? [] _errorKeys;

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of the error.
		/// </summary>
		/// <remarks>This method will take a reference to the existing <paramref name="valueMapper"/> and
		/// <paramref name="axisNames"/> parameters, but will copy the <paramref name="errorKeys"/> array.
		/// This is necessary to simplify the initial construction of the underlying array</remarks>
		/// <param name="valueMapper"></param>
		/// <param name="axisNames"></param>
		/// <param name="errorKeys"></param>
		/// <param name="errorMessage"></param>
		public Error( ValueMapper valueMapper, string [] axisNames, uint? [] errorKeys, string errorMessage )
		{
			_valueMapper = valueMapper;
			_axisNames = axisNames;
			this.ErrorMessage = errorMessage;

			_errorKeys = new uint? [ errorKeys.Length ];
			for( int i = 0; i < errorKeys.Length; i++ )
				_errorKeys [ i ] = errorKeys [ i ];
		}

		#endregion

		#region IError Members

		public ICollection<string> ErrorAxes
		{
			get
			{
				return new HashSet<string>( _axisNames.Where( ( axisName, idx ) => _errorKeys [ idx ].HasValue ) );
			}
		}

		public string ErrorMessage { get; private set; }

		public object GetErrorKey( string axisName )
		{
			for( int i = 0; i < _axisNames.Length; i++ )
			{
				if( _errorKeys [ i ].HasValue &&
					_axisNames [ i ] == axisName )
					return _valueMapper.GetObject( _errorKeys [ i ].Value );
			}

			throw new InvalidOperationException( $"No error key found for axis '{axisName}'" );
		}

		#endregion
	}
}
