namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal enum EReStructuredTextToken
	{
		Undefined,
		CodesysCodeBlock,
		CodesysRanges,
		Note,
		Replace,
		GridTable,
		SimpleTable,
		Text,
		Hyperlink,
		Admonition,
		UnspecifiedMarkup
	}
}
