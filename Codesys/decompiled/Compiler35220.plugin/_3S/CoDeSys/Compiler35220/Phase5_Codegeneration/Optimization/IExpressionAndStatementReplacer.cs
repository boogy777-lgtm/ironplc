using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000276 RID: 630
	public interface IExpressionAndStatementReplacer : ISpecificExpressionReplacer
	{
		// Token: 0x06002802 RID: 10242
		_IStatement TakeCurrentStatementToReplace();

		// Token: 0x06002803 RID: 10243
		_IStatement TakeCurrentStatementToInsert();

		// Token: 0x06002804 RID: 10244
		_IExpression ReplaceAssignmentOnStatementPosition(_IAssignmentExpression assign);
	}
}
