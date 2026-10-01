using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000220 RID: 544
	internal class BreakpointStatement_Green : Statement_Green, _IBreakPointStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IBreakPointStatement
	{
		// Token: 0x060023FA RID: 9210 RVA: 0x0005BF73 File Offset: 0x0005AF73
		public BreakpointStatement_Green(long bpPosition, long successorPosition)
		{
			this.BPPosition = bpPosition;
			this.SuccessorPosition = successorPosition;
		}

		// Token: 0x17000A24 RID: 2596
		// (get) Token: 0x060023FB RID: 9211 RVA: 0x0005BF89 File Offset: 0x0005AF89
		// (set) Token: 0x060023FC RID: 9212 RVA: 0x0005BF91 File Offset: 0x0005AF91
		public long BPPosition { get; set; }

		// Token: 0x17000A25 RID: 2597
		// (get) Token: 0x060023FD RID: 9213 RVA: 0x0005BF9A File Offset: 0x0005AF9A
		// (set) Token: 0x060023FE RID: 9214 RVA: 0x0005BFA2 File Offset: 0x0005AFA2
		public long SuccessorPosition { get; set; }

		// Token: 0x060023FF RID: 9215 RVA: 0x00014539 File Offset: 0x00013539
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002400 RID: 9216 RVA: 0x0005BFAB File Offset: 0x0005AFAB
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}
	}
}
