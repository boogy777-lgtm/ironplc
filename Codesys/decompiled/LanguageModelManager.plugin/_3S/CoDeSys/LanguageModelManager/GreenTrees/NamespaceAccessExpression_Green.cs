using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F8 RID: 504
	internal class NamespaceAccessExpression_Green : Expression_Green, _INamespaceAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INamespaceAccessExpression
	{
		// Token: 0x0600227E RID: 8830 RVA: 0x0005AD2F File Offset: 0x00059D2F
		internal NamespaceAccessExpression_Green(_IExpression expNamespace, _IExpression expAccess)
		{
			this._expNamespace = expNamespace;
			this._expAccess = expAccess;
		}

		// Token: 0x17000996 RID: 2454
		// (get) Token: 0x0600227F RID: 8831 RVA: 0x0005AD45 File Offset: 0x00059D45
		public IExpression Access
		{
			get
			{
				return this._expAccess;
			}
		}

		// Token: 0x17000997 RID: 2455
		// (get) Token: 0x06002280 RID: 8832 RVA: 0x0005AD4D File Offset: 0x00059D4D
		public IExpression Namespace
		{
			get
			{
				return this._expNamespace;
			}
		}

		// Token: 0x17000998 RID: 2456
		// (get) Token: 0x06002281 RID: 8833 RVA: 0x0005AD45 File Offset: 0x00059D45
		// (set) Token: 0x06002282 RID: 8834 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Access
		{
			get
			{
				return this._expAccess;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000999 RID: 2457
		// (get) Token: 0x06002283 RID: 8835 RVA: 0x0005AD4D File Offset: 0x00059D4D
		// (set) Token: 0x06002284 RID: 8836 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Namespace
		{
			get
			{
				return this._expNamespace;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002285 RID: 8837 RVA: 0x000105A0 File Offset: 0x0000F5A0
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351500 exprementVisitor = visitor as IExprementVisitor351500;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x06002286 RID: 8838 RVA: 0x000105BC File Offset: 0x0000F5BC
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor5 exprVisitor = visitor as IExprVisitor5;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x040006A1 RID: 1697
		private readonly _IExpression _expNamespace;

		// Token: 0x040006A2 RID: 1698
		private readonly _IExpression _expAccess;
	}
}
