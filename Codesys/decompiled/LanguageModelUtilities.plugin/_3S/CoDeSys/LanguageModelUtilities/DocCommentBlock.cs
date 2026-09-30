using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{KindOf}: {Text}")]
	internal sealed class DocCommentBlock
	{
		internal string Text { get; set; }

		internal EReStructuredTextToken KindOf { get; private set; }

		internal DocCommentBlock(string stText, EReStructuredTextToken eKindOf)
		{
			Text = stText;
			KindOf = eKindOf;
		}
	}
}
