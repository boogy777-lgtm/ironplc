using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using \u0019;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services.UpToDateChecks
{
	// Token: 0x0200011D RID: 285
	internal sealed class GlobalErrorChangeChecker : global::\u000F.\u0006
	{
		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x06001495 RID: 5269 RVA: 0x0003CCCC File Offset: 0x0003AECC
		internal static global::\u000F.\u0006 Instance { get; } = new GlobalErrorChangeChecker();

		// Token: 0x06001496 RID: 5270 RVA: 0x0003CCD4 File Offset: 0x0003AED4
		private GlobalErrorChangeChecker()
		{
		}

		// Token: 0x06001497 RID: 5271 RVA: 0x0003CCDC File Offset: 0x0003AEDC
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !GlobalErrorChangeChecker.\u0001(\u0002.CompileContext).Any<_ICompilerMessage>() || !\u0002.Strategy.GlobalError() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x06001498 RID: 5272 RVA: 0x0003CD0C File Offset: 0x0003AF0C
		private static IEnumerable<_ICompilerMessage> \u0001(_ICompileContext \u0002)
		{
			return \u0019.\u0001.\u0001(\u0002).Where(new Func<_ICompilerMessage, bool>(GlobalErrorChangeChecker.<>c.<>9.\u0001));
		}

		// Token: 0x04000390 RID: 912
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
