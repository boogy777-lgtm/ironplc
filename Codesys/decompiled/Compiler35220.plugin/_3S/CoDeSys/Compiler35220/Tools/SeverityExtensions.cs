using System;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000055 RID: 85
	public static class SeverityExtensions
	{
		// Token: 0x06000611 RID: 1553 RVA: 0x0000CAC4 File Offset: 0x0000ACC4
		public static Severity GetSeverity(this Severity severity, MessageId mid)
		{
			if (!severity.IsWarningAsError(mid))
			{
				return severity;
			}
			return Severity.Error;
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x0000CAD4 File Offset: 0x0000ACD4
		public static bool IsWarningAsError(this Severity severity, MessageId mid)
		{
			return Severity.SuppressedWarning != severity && Severity.Warning == severity && APEnvironmentFacade.Instance.IsWarningAsError(mid);
		}
	}
}
