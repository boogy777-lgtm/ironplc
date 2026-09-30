using System;
using System.Collections.Generic;
using \u0008;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0011
{
	// Token: 0x020002D0 RID: 720
	internal sealed class \u0012 : global::\u0008.\u0010
	{
		// Token: 0x06002B64 RID: 11108 RVA: 0x00098AF4 File Offset: 0x00096CF4
		public \u0012(\u0081.\u0010 \u0096\u0007, ISpecificExpressionReplacer \u0018\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0096\u0007, \u0018\u0006, \u0083\u0005)
		{
		}

		// Token: 0x06002B65 RID: 11109 RVA: 0x00098B00 File Offset: 0x00096D00
		public override void \u0001(_ICallExpression \u0002)
		{
			base.\u0001(\u0002);
			IList<_IExpression> outputs = \u0002.Outputs;
			for (int i = 0; i < outputs.Count; i++)
			{
				if (outputs[i] != null)
				{
					_IExpression exp = this.\u0001(outputs[i], true);
					\u0002.SetFormalOutput(exp, i);
				}
			}
		}
	}
}
