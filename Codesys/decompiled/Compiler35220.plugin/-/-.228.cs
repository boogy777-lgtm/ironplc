using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0011;
using \u001E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0007
{
	// Token: 0x02000278 RID: 632
	internal sealed class \u000F : AbstractStatementReplacer, IReplacer
	{
		// Token: 0x17000730 RID: 1840
		// (get) Token: 0x06002808 RID: 10248 RVA: 0x0008B7C0 File Offset: 0x000899C0
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x17000731 RID: 1841
		// (get) Token: 0x06002809 RID: 10249 RVA: 0x0008B7C8 File Offset: 0x000899C8
		private StatementReplacerVisitor Visitor { get; }

		// Token: 0x17000732 RID: 1842
		// (get) Token: 0x0600280A RID: 10250 RVA: 0x0008B7D0 File Offset: 0x000899D0
		private global::\u0011.\u0006 Parser { get; }

		// Token: 0x0600280B RID: 10251 RVA: 0x0008B7D8 File Offset: 0x000899D8
		internal \u000F(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
			this.Visitor = new \u0082.\u0010(this, \u0083\u0005);
			this.Parser = new global::\u0011.\u0006("");
		}

		// Token: 0x0600280C RID: 10252 RVA: 0x0008B804 File Offset: 0x00089A04
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new global::\u0007.\u000F(\u0002), new \u001E.\u000F());
		}

		// Token: 0x0600280D RID: 10253 RVA: 0x0008B818 File Offset: 0x00089A18
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x0600280E RID: 10254 RVA: 0x0008B828 File Offset: 0x00089A28
		public override _IStatement visit(_IIfStatement ifst)
		{
			IfStatementConverter.ReplaceElseIfs(ifst);
			return null;
		}

		// Token: 0x04000769 RID: 1897
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x0400076A RID: 1898
		[CompilerGenerated]
		private readonly StatementReplacerVisitor \u0001;

		// Token: 0x0400076B RID: 1899
		[CompilerGenerated]
		private readonly global::\u0011.\u0006 \u0001;
	}
}
