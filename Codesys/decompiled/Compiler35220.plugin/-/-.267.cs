using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x020002CE RID: 718
	internal sealed class \u0013 : AbstractToVisitchecker
	{
		// Token: 0x06002B5A RID: 11098 RVA: 0x000989C8 File Offset: 0x00096BC8
		internal \u0013(\u0011 \u0083\u0005)
		{
			this.\u0001 = \u0083\u0005;
		}

		// Token: 0x06002B5B RID: 11099 RVA: 0x000989D8 File Offset: 0x00096BD8
		public override bool ToVisit(_IVariableExpression variable)
		{
			_IVariable u = variable.GetVariable(this.\u0001._Scope) as _IVariable;
			return this.\u0001(u);
		}

		// Token: 0x06002B5C RID: 11100 RVA: 0x00098A08 File Offset: 0x00096C08
		public override bool ToVisit(_ICompoAccessExpression compo)
		{
			_IVariable u = compo._Right.GetVariable(this.\u0001._Scope) as _IVariable;
			return this.\u0001(u);
		}

		// Token: 0x06002B5D RID: 11101 RVA: 0x00098A3C File Offset: 0x00096C3C
		private bool \u0001(IVariable \u0002)
		{
			return \u0002 != null && \u0002.GetFlag(VarFlag.TaskLocal);
		}

		// Token: 0x04000846 RID: 2118
		private readonly \u0011 \u0001;
	}
}
