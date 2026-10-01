using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000058 RID: 88
	[TypeGuid("{17D1B1ED-EA6E-4BED-B86E-7A08A803A714}")]
	[StorageVersion("3.4.5.0-3.4.999.999;3.5.2.0")]
	public class HasConstantValueExpression : PragmaExpression, _IHasConstantValueExpression2, _IHasConstantValueExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasConstantValueExpression2, IHasConstantValueExpression, IHasConstantValueExpression3
	{
		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000532 RID: 1330 RVA: 0x0000EF1D File Offset: 0x0000DF1D
		// (set) Token: 0x06000533 RID: 1331 RVA: 0x0000EF25 File Offset: 0x0000DF25
		[DefaultSerialization("ValueExpression")]
		[StorageVersion("3.5.20.0")]
		[Obfuscation(Feature = "rename")]
		[StorageDefaultValue(null)]
		private _IExpression ValueExpression
		{
			get
			{
				return this.m_Value;
			}
			set
			{
				this.m_Value = value;
			}
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x0000EF2E File Offset: 0x0000DF2E
		public HasConstantValueExpression()
		{
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x0000EF41 File Offset: 0x0000DF41
		public HasConstantValueExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x0000EF55 File Offset: 0x0000DF55
		public HasConstantValueExpression(IToken token, _IExpression constant, _IExpression value) : base(token)
		{
			this.m_Constant = constant;
			this.m_Value = value;
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0000EF77 File Offset: 0x0000DF77
		public IExpression Constant
		{
			get
			{
				return this.m_Constant;
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x0000EF77 File Offset: 0x0000DF77
		// (set) Token: 0x06000539 RID: 1337 RVA: 0x0000EF7F File Offset: 0x0000DF7F
		public _IExpression _Constant
		{
			get
			{
				return this.m_Constant;
			}
			set
			{
				this.m_Constant = value;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x0600053A RID: 1338 RVA: 0x0000EF88 File Offset: 0x0000DF88
		public ILiteralExpression ConstantValue
		{
			get
			{
				return this._ConstantValue as ILiteralExpression;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600053B RID: 1339 RVA: 0x0000EF95 File Offset: 0x0000DF95
		public IExpression ConstantValueExpression
		{
			get
			{
				return this._ConstantValue;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x0000EF1D File Offset: 0x0000DF1D
		// (set) Token: 0x0600053D RID: 1341 RVA: 0x0000EF25 File Offset: 0x0000DF25
		public _IExpression _ConstantValue
		{
			get
			{
				return this.m_Value;
			}
			set
			{
				this.m_Value = value;
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x0000EF9D File Offset: 0x0000DF9D
		public Operator OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x0600053F RID: 1343 RVA: 0x0000EF9D File Offset: 0x0000DF9D
		// (set) Token: 0x06000540 RID: 1344 RVA: 0x0000EFA5 File Offset: 0x0000DFA5
		public Operator _OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
			set
			{
				this.m_opComparison = value;
			}
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x0000EFAE File Offset: 0x0000DFAE
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0000EFB7 File Offset: 0x0000DFB7
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0000EFC0 File Offset: 0x0000DFC0
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor2 exprVisitor = visitor as IExprVisitor2;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x0000EFE0 File Offset: 0x0000DFE0
		public override _IExprement Duplicate()
		{
			HasConstantValueExpression hasConstantValueExpression = new HasConstantValueExpression();
			base.DuplicateCommon(hasConstantValueExpression);
			if (this.m_Constant != null)
			{
				hasConstantValueExpression.m_Constant = (Expression)this.m_Constant.Duplicate();
			}
			if (this.m_Value != null)
			{
				hasConstantValueExpression.m_Value = (Expression)this.m_Value.Duplicate();
			}
			hasConstantValueExpression.m_opComparison = this.OpComparison;
			return hasConstantValueExpression;
		}

		// Token: 0x040000BC RID: 188
		[DefaultSerialization("Constant")]
		[StorageVersion("3.4.5.0-3.4.999.999;3.5.2.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_Constant;

		// Token: 0x040000BD RID: 189
		[DefaultSerialization("Value")]
		[StorageVersion("3.4.5.0-3.4.999.999;3.5.2.0-3.5.19.999")]
		[Obfuscation(Feature = "rename")]
		[StorageDefaultValue(null)]
		private _IExpression m_Value;

		// Token: 0x040000BE RID: 190
		[DefaultSerialization("Comparison")]
		[StorageVersion("3.5.7.0")]
		[StorageDefaultValue(Operator.Equal)]
		[Obfuscation(Feature = "rename")]
		private Operator m_opComparison = Operator.Equal;
	}
}
