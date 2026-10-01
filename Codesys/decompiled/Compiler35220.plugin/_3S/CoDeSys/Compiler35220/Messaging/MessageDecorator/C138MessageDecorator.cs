using System;
using System.Linq;
using System.Text;
using \u0004;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Messaging.MessageDecorator
{
	// Token: 0x020003A2 RID: 930
	internal sealed class C138MessageDecorator : global::\u0004.\u0017
	{
		// Token: 0x060035EF RID: 13807 RVA: 0x000D79FC File Offset: 0x000D5BFC
		internal C138MessageDecorator(ISignature signInit, _IVariable variable)
		{
			this.\u0001 = signInit;
			this.\u0001 = variable;
		}

		// Token: 0x060035F0 RID: 13808 RVA: 0x000D7A14 File Offset: 0x000D5C14
		private static void \u0001(StringBuilder \u0002)
		{
			if ('.' != \u0002[\u0002.Length - 1])
			{
				\u0002.Append(".");
			}
			\u0002.Append(" ");
		}

		// Token: 0x060035F1 RID: 13809 RVA: 0x000D7A40 File Offset: 0x000D5C40
		private string \u0001(IVariable[] \u0002)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < \u0002.Length; i++)
			{
				if (0 < i)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(\u0002[i].Type);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060035F2 RID: 13810 RVA: 0x000D7A88 File Offset: 0x000D5C88
		public string \u0001(string \u0002)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(\u0002);
			C138MessageDecorator.\u0001(stringBuilder);
			IVariable[] array = this.\u0001.AllInputs.Where(new Func<IVariable, bool>(C138MessageDecorator.<>c.<>9.\u0001)).ToArray<IVariable>();
			stringBuilder.AppendFormat(global::\u0011.\u0001.C138ErrMsgRequiresNInputs, array.Length);
			C138MessageDecorator.\u0001(stringBuilder);
			stringBuilder.AppendFormat(global::\u0011.\u0001.C138ErrMsgCheckSyntax, this.\u0001.OrgName, this.\u0001.Type, this.\u0001(array));
			return stringBuilder.ToString();
		}

		// Token: 0x04000A7D RID: 2685
		private readonly ISignature \u0001;

		// Token: 0x04000A7E RID: 2686
		private readonly _IVariable \u0001;
	}
}
