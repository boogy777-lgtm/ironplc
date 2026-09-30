using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using \u0019;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0004
{
	// Token: 0x02000122 RID: 290
	internal sealed class \u0002 : global::\u000F.\u0006
	{
		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x060014B2 RID: 5298 RVA: 0x0003D268 File Offset: 0x0003B468
		internal static global::\u000F.\u0006 Instance { get; } = new global::\u0004.\u0002();

		// Token: 0x060014B3 RID: 5299 RVA: 0x0003D270 File Offset: 0x0003B470
		private \u0002()
		{
		}

		// Token: 0x060014B4 RID: 5300 RVA: 0x0003D278 File Offset: 0x0003B478
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			\u0002.DetailedPrecompileCheck = false;
			uint num = \u0019.\u0001.\u0001(new _IPreCompileContext[]
			{
				\u0002.PreCompileContext,
				\u0002.PreCompileContextPool
			});
			_ICompileContext2 icompileContext = \u0002.CompileContext as _ICompileContext2;
			if (icompileContext != null && num != icompileContext.PrecompileContextNamesChecksum)
			{
				\u0002.DetailedPrecompileCheck = true;
				if (\u0002.Strategy.PrecomNameHashChanged())
				{
					return \u0002.Strategy.IsUpToDate;
				}
			}
			return true;
		}

		// Token: 0x04000397 RID: 919
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
