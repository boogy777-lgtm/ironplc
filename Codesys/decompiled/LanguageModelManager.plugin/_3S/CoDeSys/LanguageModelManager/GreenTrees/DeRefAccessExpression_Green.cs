using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020C RID: 524
	internal class DeRefAccessExpression_Green : Expression_Green, _IDeRefAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDeRefAccessExpression
	{
		// Token: 0x0600237B RID: 9083 RVA: 0x0005BC21 File Offset: 0x0005AC21
		internal DeRefAccessExpression_Green(_IExpression exp)
		{
			this.m_exp = exp;
		}

		// Token: 0x0600237C RID: 9084 RVA: 0x0000E3EE File Offset: 0x0000D3EE
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600237D RID: 9085 RVA: 0x0000E400 File Offset: 0x0000D400
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x170009FD RID: 2557
		// (get) Token: 0x0600237E RID: 9086 RVA: 0x0005BC30 File Offset: 0x0005AC30
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x170009FE RID: 2558
		// (get) Token: 0x0600237F RID: 9087 RVA: 0x0005BC38 File Offset: 0x0005AC38
		// (set) Token: 0x06002380 RID: 9088 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Base
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression_Green();
				}
				return this.m_exp;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009FF RID: 2559
		// (get) Token: 0x06002381 RID: 9089 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x06002382 RID: 9090 RVA: 0x0005A471 File Offset: 0x00059471
		public IDeRefExprInfo DeRefInfo
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006CA RID: 1738
		protected _IExpression m_exp;
	}
}
