using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	public class BuilderException : Exception
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public BuilderException(string stMessage)
			: base(stMessage)
		{
		}
	}
}
