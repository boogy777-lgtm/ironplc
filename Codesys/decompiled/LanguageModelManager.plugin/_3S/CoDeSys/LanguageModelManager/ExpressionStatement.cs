using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008F RID: 143
	[TypeGuid("{5fddf225-02a5-475d-8703-90944d9293f5}")]
	[StorageVersion("3.3.0.0")]
	public class ExpressionStatement : PositionStatement, _IExpressionStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IExpressionStatement
	{
		// Token: 0x060008D6 RID: 2262 RVA: 0x000149DB File Offset: 0x000139DB
		public ExpressionStatement()
		{
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0001506B File Offset: 0x0001406B
		internal ExpressionStatement(_IExpression exp)
		{
			this._Expr = exp;
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0001507A File Offset: 0x0001407A
		internal ExpressionStatement(_IExpression exp, IToken token) : base(token)
		{
			this._Expr = exp;
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x060008D9 RID: 2265 RVA: 0x0001508A File Offset: 0x0001408A
		public IExpression Expr
		{
			get
			{
				return this._Expr;
			}
		}

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x060008DA RID: 2266 RVA: 0x00015092 File Offset: 0x00014092
		// (set) Token: 0x060008DB RID: 2267 RVA: 0x000150A8 File Offset: 0x000140A8
		public _IExpression _Expr
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression();
				}
				return this.m_exp;
			}
			set
			{
				this.m_exp = value;
			}
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x000150B1 File Offset: 0x000140B1
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x000150BA File Offset: 0x000140BA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x000150C3 File Offset: 0x000140C3
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x000150CC File Offset: 0x000140CC
		public override _IExprement Duplicate()
		{
			ExpressionStatement expressionStatement = new ExpressionStatement();
			this.DuplicateCommon(expressionStatement);
			expressionStatement._Expr = (this.m_exp.Duplicate() as _IExpression);
			return expressionStatement;
		}

		// Token: 0x04000130 RID: 304
		[DefaultSerialization("Expression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_exp;
	}
}
