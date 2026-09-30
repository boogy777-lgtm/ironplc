using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("({Text}, {UserdefType}")]
	internal class UserdefTypeAttributedString : AttributedString, IUserdefTypeAttributedString, IAttributedString
	{
		public IUserdefType UserdefType { get; private set; }

		public bool Available { get; private set; }

		internal UserdefTypeAttributedString(string stText, IUserdefType udt, bool bAvailable)
			: base(stText)
		{
			UserdefType = udt;
			Available = bAvailable;
		}
	}
}
