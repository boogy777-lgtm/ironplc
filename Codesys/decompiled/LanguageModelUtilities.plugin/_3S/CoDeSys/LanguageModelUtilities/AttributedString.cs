using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("({Text})")]
	internal abstract class AttributedString : IAttributedString
	{
		public string Text { get; private set; }

		protected AttributedString(string stText)
		{
			Text = stText;
		}
	}
}
