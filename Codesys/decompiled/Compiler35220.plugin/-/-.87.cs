using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u001F
{
	// Token: 0x02000114 RID: 276
	internal abstract class \u0004
	{
		// Token: 0x06001441 RID: 5185 RVA: 0x0003B404 File Offset: 0x00039604
		internal static bool \u0001(ISignature \u0002)
		{
			foreach (string item in \u0002.Attributes)
			{
				if (\u0004.\u0001.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400037A RID: 890
		private static readonly HashSet<string> \u0001 = new HashSet<string>
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
