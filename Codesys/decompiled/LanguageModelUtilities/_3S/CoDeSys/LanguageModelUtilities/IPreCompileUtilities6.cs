using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities6 : IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		void DoStandardTreeTraversal(IExprementVisitorNoTraversion visitor, ISignature4 localSignature, IPreCompileContext9 precomApp, IExprement expToVisit);

		void DoStatementTreeTraversal(IStatementVisitorNoTraversion visitor, IStatement statementToVisit);

		void VisitAllVariables(IVariableVisitor visitor, ISignature4 localSignature, IPreCompileContext9 precomApp, IExprement expToVisit);

		IType ResolveAliasType(IEvaluationContext context, IType aliasType);

		void EnforcePresenceOfAllPreCompileContexts(IProgressCallback callback);
	}
}
