using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class TooOldCompilerversionException : Exception
	{
		public readonly Version _requiredVersion;

		internal TooOldCompilerversionException(Version requiredVersion)
		{
			_requiredVersion = requiredVersion;
		}
	}
}
