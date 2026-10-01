using System;
using System.Collections.Generic;

namespace CODESYS.Parser35210.Scanner
{
	internal static class ReservedUnusedKeywords
	{
		private static HashSet<string> Keywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CHAR", "WCHAR", "ANY_DERIVED", "ANY_ELEMENTARY", "ANY_MAGNITUDE", "ANY_SIGNED", "ANY_DURATION", "ANY_CHARS", "ANY_CHAR", "CHAR_TO",
			"TO_CHAR", "WCHAR_TO", "TO_WCHAR", "ATAN2", "USING", "CLASS"
		};


		public static bool IsReservedUnusedKeyword(string stToken)
		{
			return Keywords.Contains(stToken);
		}
	}
}
