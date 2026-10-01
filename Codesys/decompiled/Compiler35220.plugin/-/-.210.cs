using System;
using \u0006;
using \u0013;
using \u001E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x02000248 RID: 584
	internal sealed class \u000E : global::\u0006.\u0002
	{
		// Token: 0x0600265B RID: 9819 RVA: 0x00085E64 File Offset: 0x00084064
		private \u000E(_ICompiledPOU \u0012\u0002, \u001E.\u000E \u0018\u0004)
		{
			this.\u0001 = \u0012\u0002;
			this.\u0001 = \u0018\u0004;
		}

		// Token: 0x0600265C RID: 9820 RVA: 0x00085E7C File Offset: 0x0008407C
		public override void \u0001(_ITryCatchStatement \u0002)
		{
			IBreakpoint breakpoint = this.\u0001.\u0001(\u0002.Subroutine);
			Debug.\u0001(breakpoint != null);
			if (breakpoint != null)
			{
				this.\u0001.AddTryCatchCodeAddressIndex(breakpoint.Offset, \u0002.Index);
			}
		}

		// Token: 0x0600265D RID: 9821 RVA: 0x00085EC0 File Offset: 0x000840C0
		public static void \u0001(_ICompiledPOU \u0002, \u001E.\u000E \u0003)
		{
			if (\u0002.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
			{
				IStatementTraverser ivisit = new \u0013.\u0001(new \u001A.\u000E(\u0002, \u0003));
				((_IStatement)\u0002.ParseTree).Accept(ivisit);
			}
		}

		// Token: 0x04000702 RID: 1794
		private new readonly _ICompiledPOU \u0001;

		// Token: 0x04000703 RID: 1795
		private new readonly \u001E.\u000E \u0001;
	}
}
