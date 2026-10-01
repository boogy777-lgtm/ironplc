using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x020002BC RID: 700
	internal sealed class \u000F : AbstractToVisitchecker
	{
		// Token: 0x1700076F RID: 1903
		// (get) Token: 0x06002ADB RID: 10971 RVA: 0x00096C28 File Offset: 0x00094E28
		private \u0011 Context { get; }

		// Token: 0x06002ADC RID: 10972 RVA: 0x00096C30 File Offset: 0x00094E30
		internal \u000F(\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
		}

		// Token: 0x06002ADD RID: 10973 RVA: 0x00096C40 File Offset: 0x00094E40
		public override bool ToVisit(_IVariableExpression variable)
		{
			_IVariable u = variable.GetVariable(this.Context._Scope) as _IVariable;
			return this.\u0001(u);
		}

		// Token: 0x06002ADE RID: 10974 RVA: 0x00096C70 File Offset: 0x00094E70
		public override bool ToVisit(_ICompoAccessExpression compo)
		{
			_IVariable u = compo._Right.GetVariable(this.Context._Scope) as _IVariable;
			return this.\u0001(u);
		}

		// Token: 0x06002ADF RID: 10975 RVA: 0x00096CA4 File Offset: 0x00094EA4
		private bool \u0001(_IVariable \u0002)
		{
			return \u0002 != null && (\u0002.IsProperty || \u0002.HasAttribute(CompileAttributes.GET_BITACCESS) || \u0002.HasAttribute(CompileAttributes.DEVICE_PARAMETER));
		}

		// Token: 0x0400081B RID: 2075
		[CompilerGenerated]
		private readonly \u0011 \u0001;
	}
}
