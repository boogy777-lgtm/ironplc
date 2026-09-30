using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHelper5 : ICompilerHelper4, ICompilerHelper3, ICompilerHelper2, ICompilerHelper
	{
		IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature, EPouScopeFlags eForWhichScope);

		IExpressionInfo GetExpressionInfo(_IPreCompileContext precomLocal, Guid guidSignature, string stExpression, bool bImplicit);
	}
}
