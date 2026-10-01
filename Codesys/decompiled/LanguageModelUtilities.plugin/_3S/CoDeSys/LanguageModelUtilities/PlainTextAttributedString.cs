namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class PlainTextAttributedString : AttributedString, IPlainTextAttributedString, IAttributedString
	{
		internal PlainTextAttributedString(string stText)
			: base(stText)
		{
		}
	}
}
