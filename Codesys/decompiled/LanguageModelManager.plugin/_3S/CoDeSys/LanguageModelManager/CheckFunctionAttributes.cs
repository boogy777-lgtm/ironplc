using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C0 RID: 192
	internal abstract class CheckFunctionAttributes
	{
		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000B84 RID: 2948 RVA: 0x0001D299 File Offset: 0x0001C299
		public static IEnumerable<string> AllCheckFunctionAttributes
		{
			get
			{
				foreach (string text in CheckFunctionAttributes.checkAttributes)
				{
					yield return text;
				}
				HashSet<string>.Enumerator enumerator = default(HashSet<string>.Enumerator);
				yield break;
				yield break;
			}
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0001D2A4 File Offset: 0x0001C2A4
		public static bool IsCheckFunction(ISignature sign)
		{
			foreach (string item in sign.Attributes)
			{
				if (CheckFunctionAttributes.checkAttributes.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040001EB RID: 491
		private static readonly HashSet<string> checkAttributes = new HashSet<string>
		{
			CompileAttributes.ATTRIBUTE_CHECK_BOUNDS,
			CompileAttributes.ATTRIBUTE_CHECK_POINTER,
			CompileAttributes.ATTRIBUTE_CHECK_RANGE_UNSIGNED,
			CompileAttributes.ATTRIBUTE_CHECK_RANGE_SIGNED,
			CompileAttributes.ATTRIBUTE_CHECK_LRANGE_UNSIGNED,
			CompileAttributes.ATTRIBUTE_CHECK_LRANGE_SIGNED,
			CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL64,
			CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL32,
			CompileAttributes.ATTRIBUTE_CHECK_DIV_INT64,
			CompileAttributes.ATTRIBUTE_CHECK_DIV_INT32
		};
	}
}
