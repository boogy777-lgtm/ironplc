using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000282 RID: 642
	public interface IExpressionReplacer
	{
		// Token: 0x060028A5 RID: 10405
		_IExpression ReplaceExpression(_IExpression expression, bool bReadAccess = true);
	}
}
