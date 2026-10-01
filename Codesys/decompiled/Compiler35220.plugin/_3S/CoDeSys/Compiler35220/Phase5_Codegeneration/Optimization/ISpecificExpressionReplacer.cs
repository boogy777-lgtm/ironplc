using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000251 RID: 593
	public interface ISpecificExpressionReplacer
	{
		// Token: 0x060026D3 RID: 9939
		_IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess);

		// Token: 0x060026D4 RID: 9940
		_IExpression ReplaceIndexAccessExpression(_IIndexAccessExpression indexAccessExpression, bool bReadAccess);

		// Token: 0x060026D5 RID: 9941
		_IExpression ReplaceDeRefAccessExpression(_IDeRefAccessExpression deRefAccessExpression, bool bReadAccess);

		// Token: 0x060026D6 RID: 9942
		_IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess);

		// Token: 0x060026D7 RID: 9943
		_IExpression ReplaceAssignmentExpression(_IAssignmentExpression assignmentExpression);

		// Token: 0x060026D8 RID: 9944
		_IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression);

		// Token: 0x060026D9 RID: 9945
		_IExpression ReplaceConversionExpression(_IConversionExpression conversionExpression);

		// Token: 0x060026DA RID: 9946
		_IExpression ReplaceThisExpression(_IThisExpression thisExpression);

		// Token: 0x060026DB RID: 9947
		_IExpression ReplaceBaseExpression(_IBaseExpression baseExpression);

		// Token: 0x060026DC RID: 9948
		_IExpression ReplaceCurrentTaskExpression(_ICurrentTaskExpression currentTaskExpression);

		// Token: 0x060026DD RID: 9949
		_IExpression ReplaceCallExpression(_ICallExpression callExpression);

		// Token: 0x060026DE RID: 9950
		_IExpression ReplacePartialAccessExpression(_IPartialAccessExpression partialAccessExpression, bool bReadAccess);
	}
}
