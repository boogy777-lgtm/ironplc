using System;
using \u0006;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x0200027B RID: 635
	internal sealed class \u0010 : StatementReplacerVisitor
	{
		// Token: 0x06002826 RID: 10278 RVA: 0x0008B868 File Offset: 0x00089A68
		internal \u0010(IStatementVisitor<_IStatement> \u0018\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0018\u0006, \u0083\u0005)
		{
		}

		// Token: 0x06002827 RID: 10279 RVA: 0x0008B874 File Offset: 0x00089A74
		public override _IStatement visit(_IIfStatement ifst)
		{
			ifst._IfThen.\u0001(this);
			if (ifst._IfElse != null)
			{
				ifst._IfElse.\u0001(this);
			}
			if (ifst._ElseIf != null && ifst._ElseIf.Count > 0)
			{
				foreach (_IElseIf ielseIf in ifst._ElseIf)
				{
					ielseIf._Controlled.\u0001(this);
				}
			}
			return this._replacer.visit(ifst);
		}
	}
}
