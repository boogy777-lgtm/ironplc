using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000207 RID: 519
	internal class TypeExpression_Green : Expression_Green, _ITypeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ITypeExpression
	{
		// Token: 0x06002344 RID: 9028 RVA: 0x0005BADC File Offset: 0x0005AADC
		public TypeExpression_Green(ICompiledType cType)
		{
			this._cType = cType;
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x00012F64 File Offset: 0x00011F64
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x170009E7 RID: 2535
		// (get) Token: 0x06002347 RID: 9031 RVA: 0x0005BAEB File Offset: 0x0005AAEB
		public ICompiledType2 ExpressionType
		{
			get
			{
				return this._cType as ICompiledType2;
			}
		}

		// Token: 0x170009E8 RID: 2536
		// (get) Token: 0x06002348 RID: 9032 RVA: 0x0005BAF8 File Offset: 0x0005AAF8
		// (set) Token: 0x06002349 RID: 9033 RVA: 0x0005A471 File Offset: 0x00059471
		public override ICompiledType Type
		{
			get
			{
				return this._cType;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009E9 RID: 2537
		// (get) Token: 0x0600234A RID: 9034 RVA: 0x0005BAF8 File Offset: 0x0005AAF8
		// (set) Token: 0x0600234B RID: 9035 RVA: 0x0005A471 File Offset: 0x00059471
		public override ICompiledType _CompiledType
		{
			get
			{
				return this._cType;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006C3 RID: 1731
		private readonly ICompiledType _cType;
	}
}
