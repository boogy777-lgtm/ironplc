using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FF RID: 511
	internal class CastExpression_Green : Expression_Green, _ICastExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICastExpression
	{
		// Token: 0x060022ED RID: 8941 RVA: 0x0005B750 File Offset: 0x0005A750
		public CastExpression_Green(_IExpression expWithType, _IExpression expBase, ICompiledType type)
		{
			this.m_expWithType = expWithType;
			this.m_expBase = expBase;
			this.m_type = type;
		}

		// Token: 0x060022EE RID: 8942 RVA: 0x0000C5BB File Offset: 0x0000B5BB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022EF RID: 8943 RVA: 0x0000C5CD File Offset: 0x0000B5CD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x170009C3 RID: 2499
		// (get) Token: 0x060022F0 RID: 8944 RVA: 0x0005B76D File Offset: 0x0005A76D
		// (set) Token: 0x060022F1 RID: 8945 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression ExpWithType
		{
			get
			{
				return this.m_expWithType;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009C4 RID: 2500
		// (get) Token: 0x060022F2 RID: 8946 RVA: 0x0005B775 File Offset: 0x0005A775
		// (set) Token: 0x060022F3 RID: 8947 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression BaseExpression
		{
			get
			{
				return this.m_expBase;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009C5 RID: 2501
		// (get) Token: 0x060022F4 RID: 8948 RVA: 0x0005B76D File Offset: 0x0005A76D
		public IExpression ExprWithType
		{
			get
			{
				return this.m_expWithType;
			}
		}

		// Token: 0x170009C6 RID: 2502
		// (get) Token: 0x060022F5 RID: 8949 RVA: 0x0005B775 File Offset: 0x0005A775
		public IExpression Base
		{
			get
			{
				return this.m_expBase;
			}
		}

		// Token: 0x170009C7 RID: 2503
		// (get) Token: 0x060022F6 RID: 8950 RVA: 0x0005B77D File Offset: 0x0005A77D
		// (set) Token: 0x060022F7 RID: 8951 RVA: 0x0005A471 File Offset: 0x00059471
		public ICompiledType ExplicitelySpecifiedType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006B6 RID: 1718
		private readonly _IExpression m_expWithType;

		// Token: 0x040006B7 RID: 1719
		private readonly _IExpression m_expBase;

		// Token: 0x040006B8 RID: 1720
		private readonly ICompiledType m_type;
	}
}
