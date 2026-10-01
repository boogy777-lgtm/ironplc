using System;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002F RID: 47
	internal static class CompilerMessageExtensions
	{
		// Token: 0x060001FE RID: 510 RVA: 0x00007299 File Offset: 0x00006299
		public static bool IsSuppressed(this _ICompilerMessage message)
		{
			return message.Severity == Severity.SuppressedInformation || message.Severity == Severity.SuppressedWarning;
		}
	}
}
