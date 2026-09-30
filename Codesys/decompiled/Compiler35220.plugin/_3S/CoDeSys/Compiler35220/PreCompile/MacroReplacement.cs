using System;
using \u0008;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000172 RID: 370
	public static class MacroReplacement
	{
		// Token: 0x060018F5 RID: 6389 RVA: 0x0004DCA8 File Offset: 0x0004BEA8
		public static void ReplaceMacroOperators(int nProjectHandle, Guid objectGuid, ISequenceStatement seqStmt)
		{
			MacroInfoProvider u = MacroInfoProvider.Create(nProjectHandle, objectGuid);
			\u0007.\u0001(seqStmt, u);
		}

		// Token: 0x060018F6 RID: 6390 RVA: 0x0004DCC4 File Offset: 0x0004BEC4
		public static void ReplaceMacroOperators(int nProjectHandle, ILMPOU lmpou)
		{
			MacroInfoProvider u = MacroInfoProvider.Create(nProjectHandle, lmpou);
			if (lmpou.Interface != null)
			{
				\u0007.\u0001(lmpou.Interface, u);
			}
			if (lmpou.Body != null)
			{
				\u0007.\u0001(lmpou.Body, u);
			}
		}
	}
}
