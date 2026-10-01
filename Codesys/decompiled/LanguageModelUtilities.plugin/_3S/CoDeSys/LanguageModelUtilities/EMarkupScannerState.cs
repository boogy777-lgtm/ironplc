namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal enum EMarkupScannerState
	{
		Start,
		End,
		Error,
		Dot1,
		Dot2,
		Whitespace,
		Identifier,
		Colon1,
		Colon2
	}
}
