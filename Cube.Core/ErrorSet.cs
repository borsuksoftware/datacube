using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Cube
{
	/// <summary>
	/// Standard implementation of <see cref="IErrorSet"/>
	/// </summary>
	internal class ErrorSet : IErrorSet
	{
		#region Member variables

		private ValueMapper _valueMapper;

		private string []  _axisNames;

		private Dictionary.HybridErrorsDictionary _errorsDictionary;

		#endregion

		/// <summary>
		/// Creates a new instance of <see cref="ErrorSet"/>
		/// </summary>
		/// <param name="valueMapper">The value mapper to use to encode values</param>
		/// <param name="axisNames">THe set of axes that can be used to key errors on</param>
		public ErrorSet( ValueMapper valueMapper, string [] axisNames )
		{
			if( valueMapper == null )
				throw new ArgumentNullException( nameof( valueMapper ) );

			if( axisNames == null )
				throw new ArgumentNullException( nameof( axisNames ) );

			this._valueMapper = valueMapper;
			this._axisNames = axisNames;

			this._errorsDictionary = new Dictionary.HybridErrorsDictionary( _axisNames );
		}

		#region IErrorSet Members

		public IEnumerator<IError> GetEnumerator()
		{
			var enumerator = this._errorsDictionary.GetEnumerator( null );
			while( enumerator.MoveNext() )
				yield return new Error( _valueMapper,
					_axisNames,
					enumerator.Current.Key,
					enumerator.Current.Value );
		}

		public IEnumerator<IError> GetMatchingErrors( IDictionary<string, object> errorKeys )
		{
			if( errorKeys == null || errorKeys.Count == 0 )
			{
				var enumerator = this.GetEnumerator();
				while( enumerator.MoveNext() )
					yield return enumerator.Current;

				yield break;
			}

			var convertedKeys = new Dictionary<string, uint?>();
			foreach( var pair in errorKeys )
			{
				uint id;
				if( _valueMapper.TryGetID( pair.Value, out id ))
				{
					convertedKeys [ pair.Key ] = id;
				}
				else
				{
					// We know for certain that there were no entries explicitly for the given
					// value, however, general purpose ones could still match it (think getting errors
					// for tenor = 3M, but there's a general failure for that trade)
					//
					// In order to avoid the non-threadsafety of updating the valuemapper, we need to
					// inform the underlying dictionary of the desires
					convertedKeys [ pair.Key ] = null;
				}
			}

			{
				var enumerator = this._errorsDictionary.GetEnumerator( convertedKeys );
				while( enumerator.MoveNext() )
				{
					yield return new Error( _valueMapper,
						_axisNames,
						enumerator.Current.Key,
						enumerator.Current.Value );
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void AddError( IDictionary<string, object> errorKeys, string errorMessage )
		{
			var convertedKeys = new Dictionary<string, uint>();
			foreach( var pair in errorKeys )
			{
				var id = _valueMapper.GetID( pair.Value );
				convertedKeys [ pair.Key ] = id;
			}

			_errorsDictionary.AddItem( convertedKeys, errorMessage );
		}

		#endregion
	}
}
