using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000211 RID: 529
	internal class PartialAccessExpression_Green : Expression_Green, _IPartialAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPartialAccessExpression
	{
		// Token: 0x17000A08 RID: 2568
		// (get) Token: 0x0600239D RID: 9117 RVA: 0x0005BD3A File Offset: 0x0005AD3A
		// (set) Token: 0x0600239E RID: 9118 RVA: 0x0005BD42 File Offset: 0x0005AD42
		public _IExpression _Left { get; set; }

		// Token: 0x17000A09 RID: 2569
		// (get) Token: 0x0600239F RID: 9119 RVA: 0x0005BD4B File Offset: 0x0005AD4B
		// (set) Token: 0x060023A0 RID: 9120 RVA: 0x0005BD53 File Offset: 0x0005AD53
		public DirectVariableSize PartSize { get; set; }

		// Token: 0x17000A0A RID: 2570
		// (get) Token: 0x060023A1 RID: 9121 RVA: 0x0005BD5C File Offset: 0x0005AD5C
		// (set) Token: 0x060023A2 RID: 9122 RVA: 0x0005BD64 File Offset: 0x0005AD64
		public int PartOffset { get; set; }

		// Token: 0x17000A0B RID: 2571
		// (get) Token: 0x060023A3 RID: 9123 RVA: 0x0005BD6D File Offset: 0x0005AD6D
		public IExpression Left
		{
			get
			{
				return this._Left;
			}
		}

		// Token: 0x060023A4 RID: 9124 RVA: 0x0005BD75 File Offset: 0x0005AD75
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351900 exprementVisitor = visitor as IExprementVisitor351900;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x060023A5 RID: 9125 RVA: 0x0005BD88 File Offset: 0x0005AD88
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor8 exprVisitor = visitor as IExprVisitor8;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x060023A6 RID: 9126 RVA: 0x0005BD9B File Offset: 0x0005AD9B
		public bool GetByteOffsetAndSize(IScope5 scope, out int byteOffset, out int byteSize)
		{
			byteOffset = 0;
			byteSize = 0;
			return false;
		}
	}
}
