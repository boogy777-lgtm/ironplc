using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FA RID: 506
	internal class CallExpression_Green_WithEmptyAssigns : CallExpression_Green
	{
		// Token: 0x060022AF RID: 8879 RVA: 0x0005AFCB File Offset: 0x00059FCB
		[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "call is a complex expression, constructor should contain all subexpressions")]
		public CallExpression_Green_WithEmptyAssigns(_IExpression expCallee, _IExpression expCondition, _IType typeExpected, _IExpression[] expActualParams, _IExpression[] expFormalParams, _IExpression[] expActualOutputs, _IExpression[] expFormalOutputs, _IExpression[] emptyAssigns) : base(expCallee, expCondition, typeExpected, expActualParams, expFormalParams, expActualOutputs, expFormalOutputs)
		{
			this._EmptyAssigns = emptyAssigns;
		}

		// Token: 0x170009AD RID: 2477
		// (get) Token: 0x060022B0 RID: 8880 RVA: 0x0005AFE6 File Offset: 0x00059FE6
		public override IList<_IExpression> EmptyAssigns
		{
			get
			{
				return this._EmptyAssigns;
			}
		}

		// Token: 0x060022B1 RID: 8881 RVA: 0x0005A471 File Offset: 0x00059471
		public override void SetEmptyAssign(_IExpression exp, int i)
		{
			throw new NotSupportedException();
		}

		// Token: 0x040006AA RID: 1706
		private readonly _IExpression[] _EmptyAssigns;
	}
}
