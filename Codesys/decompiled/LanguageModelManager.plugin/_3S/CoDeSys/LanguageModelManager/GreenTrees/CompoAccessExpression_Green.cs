using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020B RID: 523
	internal class CompoAccessExpression_Green : Expression_Green, _ICompoAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICompoAccessExpression, IQualifiedNameExpression
	{
		// Token: 0x06002370 RID: 9072 RVA: 0x0005BBB5 File Offset: 0x0005ABB5
		internal CompoAccessExpression_Green(_IExpression expLeft, _IExpression expRight)
		{
			this.m_exp = expLeft;
			this.m_var = expRight;
		}

		// Token: 0x170009F7 RID: 2551
		// (get) Token: 0x06002371 RID: 9073 RVA: 0x0005BBCB File Offset: 0x0005ABCB
		public IExpression Left
		{
			get
			{
				return this._Left;
			}
		}

		// Token: 0x170009F8 RID: 2552
		// (get) Token: 0x06002372 RID: 9074 RVA: 0x0005BBD3 File Offset: 0x0005ABD3
		// (set) Token: 0x06002373 RID: 9075 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Left
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

		// Token: 0x170009F9 RID: 2553
		// (get) Token: 0x06002374 RID: 9076 RVA: 0x0005BBE9 File Offset: 0x0005ABE9
		public IExpression Right
		{
			get
			{
				return this._Right;
			}
		}

		// Token: 0x170009FA RID: 2554
		// (get) Token: 0x06002375 RID: 9077 RVA: 0x0005BBF1 File Offset: 0x0005ABF1
		// (set) Token: 0x06002376 RID: 9078 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Right
		{
			get
			{
				if (this.m_var == null)
				{
					return new NullExpression_Green();
				}
				return this.m_var;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002377 RID: 9079 RVA: 0x0000CA76 File Offset: 0x0000BA76
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x0000CA88 File Offset: 0x0000BA88
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x170009FB RID: 2555
		// (get) Token: 0x06002379 RID: 9081 RVA: 0x0005BC07 File Offset: 0x0005AC07
		public string Name
		{
			get
			{
				return this._Right.ToString();
			}
		}

		// Token: 0x170009FC RID: 2556
		// (get) Token: 0x0600237A RID: 9082 RVA: 0x0005BC14 File Offset: 0x0005AC14
		public string Namespace
		{
			get
			{
				return this._Left.ToString();
			}
		}

		// Token: 0x040006C8 RID: 1736
		private readonly _IExpression m_exp;

		// Token: 0x040006C9 RID: 1737
		private readonly _IExpression m_var;
	}
}
