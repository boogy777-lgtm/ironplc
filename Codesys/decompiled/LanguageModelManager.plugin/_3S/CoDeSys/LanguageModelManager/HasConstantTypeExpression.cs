using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000057 RID: 87
	[TypeGuid("{8C6BB283-C5BD-468D-98B8-3E580C258E00}")]
	[StorageVersion("3.5.18.0")]
	public class HasConstantTypeExpression : PragmaExpression, _IHasConstantTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasConstantTypeExpression
	{
		// Token: 0x06000524 RID: 1316 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public HasConstantTypeExpression()
		{
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public HasConstantTypeExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x0000EE5B File Offset: 0x0000DE5B
		public HasConstantTypeExpression(IToken token, _IExpression constant, bool bConstantTypeReplaced) : base(token)
		{
			this.m_Constant = constant;
			this.m_bConstantTypeReplaced = bConstantTypeReplaced;
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x0000EE72 File Offset: 0x0000DE72
		public IExpression Constant
		{
			get
			{
				return this._Constant;
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000528 RID: 1320 RVA: 0x0000EE7A File Offset: 0x0000DE7A
		public bool ConstantTypeReplaced
		{
			get
			{
				return this._ConstantTypeReplaced;
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000529 RID: 1321 RVA: 0x0000EE82 File Offset: 0x0000DE82
		// (set) Token: 0x0600052A RID: 1322 RVA: 0x0000EE8A File Offset: 0x0000DE8A
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

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x0600052B RID: 1323 RVA: 0x0000EE93 File Offset: 0x0000DE93
		// (set) Token: 0x0600052C RID: 1324 RVA: 0x0000EE9B File Offset: 0x0000DE9B
		public bool _ConstantTypeReplaced
		{
			get
			{
				return this.m_bConstantTypeReplaced;
			}
			set
			{
				this.m_bConstantTypeReplaced = value;
			}
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x0000EEA4 File Offset: 0x0000DEA4
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351900 exprementVisitor = visitor as IExprementVisitor351900;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0000EEB7 File Offset: 0x0000DEB7
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0000EEC0 File Offset: 0x0000DEC0
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor8 exprVisitor = visitor as IExprVisitor8;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x0000EEE0 File Offset: 0x0000DEE0
		public override _IExprement Duplicate()
		{
			HasConstantTypeExpression hasConstantTypeExpression = new HasConstantTypeExpression();
			base.DuplicateCommon(hasConstantTypeExpression);
			hasConstantTypeExpression.m_Constant = (Expression)this.m_Constant.Duplicate();
			hasConstantTypeExpression.m_bConstantTypeReplaced = this.m_bConstantTypeReplaced;
			return hasConstantTypeExpression;
		}

		// Token: 0x040000BA RID: 186
		[DefaultSerialization("Constant")]
		[StorageVersion("3.5.18.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_Constant;

		// Token: 0x040000BB RID: 187
		[DefaultSerialization("ConstantType")]
		[StorageVersion("3.5.18.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bConstantTypeReplaced;
	}
}
