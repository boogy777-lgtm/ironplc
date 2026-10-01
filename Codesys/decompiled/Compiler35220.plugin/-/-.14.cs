using System;
using \u0004;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0011
{
	// Token: 0x02000067 RID: 103
	internal static class \u0002
	{
		// Token: 0x060007E4 RID: 2020 RVA: 0x0001064C File Offset: 0x0000E84C
		internal static void \u0001(this _ISignature \u0002, ISourcePosition \u0003, Severity \u0004, MessageId \u0005, params object[] \u0006)
		{
			\u0002.\u0001(\u0003, \u0004, \u0005, null, \u0006);
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x0001065C File Offset: 0x0000E85C
		internal static void \u0001(this _ISignature \u0002, ISourcePosition \u0003, Severity \u0004, MessageId \u0005, global::\u0004.\u0017 \u0006, params object[] \u0007)
		{
			if (\u0004 == Severity.Warning && APEnvironmentFacade.Instance.IsWarningMessageDisabled(\u0005))
			{
				\u0004 = Severity.SuppressedWarning;
			}
			if (\u0003 == null)
			{
				\u0003 = global::\u0019.\u0003.\u0001(-1, \u0002.ObjectGuid, 0L, 0, 0);
			}
			string text = string.Format(global::\u000E.\u0018.\u0001(\u0005), \u0007);
			if (\u0006 != null)
			{
				text = \u0006.\u0001(text);
			}
			\u0002.AddMessage(\u0002.CreateCompilerMessage(\u0003, text, \u0004, (int)\u0005));
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x000106C0 File Offset: 0x0000E8C0
		internal static LList<IVariable> \u0001(this _ISignature \u0002)
		{
			LList<IVariable> llist = new LList<IVariable>();
			foreach (IVariable variable in \u0002.AllLazy)
			{
				if (variable.Type.Class == TypeClass.Lazy)
				{
					llist.Add(variable);
				}
			}
			return llist;
		}
	}
}
