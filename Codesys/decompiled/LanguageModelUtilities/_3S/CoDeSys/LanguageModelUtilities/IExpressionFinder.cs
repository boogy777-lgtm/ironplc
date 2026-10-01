using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IExpressionFinder
	{
		void Initialize(IExpressionFinderExceptionHandler exceptionHandler);

		IEnumerable<IExpression> FindVariableAccesses(string stExpr, bool bExpression);

		IEnumerable<IExpression> FindVariableAccesses(IExpression expr);

		IEnumerable<IExpression> FindVariableAccesses(ISequenceStatement seqStmt);
	}
}
