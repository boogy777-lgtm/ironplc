using System;
using System.Collections.Generic;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000016 RID: 22
	internal static class ReservedUnusedKeywords
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060001CD RID: 461 RVA: 0x0000B27E File Offset: 0x0000947E
		private static HashSet<string> Keywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CHAR",
			"WCHAR",
			"ANY_DERIVED",
			"ANY_ELEMENTARY",
			"ANY_MAGNITUDE",
			"ANY_SIGNED",
			"ANY_DURATION",
			"ANY_CHARS",
			"ANY_CHAR",
			"CHAR_TO",
			"TO_CHAR",
			"WCHAR_TO",
			"TO_WCHAR",
			"ATAN2",
			"USING",
			"CLASS",
			"NAMESPACE"
		};

		// Token: 0x060001CE RID: 462 RVA: 0x0000B285 File Offset: 0x00009485
		public static bool IsReservedUnusedKeyword(string stToken)
		{
			return ReservedUnusedKeywords.Keywords.Contains(stToken);
		}
	}
}
