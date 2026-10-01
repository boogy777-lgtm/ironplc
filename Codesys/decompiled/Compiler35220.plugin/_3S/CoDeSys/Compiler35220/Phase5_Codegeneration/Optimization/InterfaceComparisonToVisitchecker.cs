using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002A9 RID: 681
	public class InterfaceComparisonToVisitchecker : AbstractToVisitchecker
	{
		// Token: 0x06002A95 RID: 10901 RVA: 0x00095148 File Offset: 0x00093348
		internal InterfaceComparisonToVisitchecker(InterfaceComparisonReplacer replacer)
		{
			this.\u0001 = replacer;
		}

		// Token: 0x06002A96 RID: 10902 RVA: 0x00095158 File Offset: 0x00093358
		public override bool ToVisit(_IOperatorExpression op)
		{
			Operator code = op.Code;
			if (code - Operator.Eq <= 1 || code - Operator.Equal <= 1)
			{
				bool flag = this.\u0001.\u0001(op._OperandsList[0]) != null;
				_IExpression iexpression = this.\u0001.\u0001(op._OperandsList[1]);
				return flag || iexpression != null;
			}
			return false;
		}

		// Token: 0x04000800 RID: 2048
		private readonly InterfaceComparisonReplacer \u0001;
	}
}
