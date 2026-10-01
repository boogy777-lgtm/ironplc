using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000222 RID: 546
	internal class IsEnumTypeExpression_Green : PragmaExpression_Green, _IIsEnumTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IIsEnumTypeExpression
	{
		// Token: 0x0600240F RID: 9231 RVA: 0x0005C002 File Offset: 0x0005B002
		public IsEnumTypeExpression_Green(ICompiledType type)
		{
			this.m_type = type;
		}

		// Token: 0x06002410 RID: 9232 RVA: 0x0000FE5F File Offset: 0x0000EE5F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002411 RID: 9233 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x17000A2D RID: 2605
		// (get) Token: 0x06002412 RID: 9234 RVA: 0x0005C011 File Offset: 0x0005B011
		// (set) Token: 0x06002413 RID: 9235 RVA: 0x0005A471 File Offset: 0x00059471
		public ICompiledType ReferencedType
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

		// Token: 0x040006E4 RID: 1764
		protected ICompiledType m_type;
	}
}
