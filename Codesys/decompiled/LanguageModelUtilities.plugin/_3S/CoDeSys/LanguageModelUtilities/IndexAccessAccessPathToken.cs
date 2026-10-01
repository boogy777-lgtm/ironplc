using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{StringRepresentation}")]
	internal sealed class IndexAccessAccessPathToken : IAccessPathToken
	{
		public string StringRepresentation => "[0]";

		public bool NeedsSeparator => false;

		internal IndexAccessAccessPathToken()
		{
		}
	}
}
