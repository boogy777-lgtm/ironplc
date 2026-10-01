using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder3 : ILanguageModelBuilder2, ILanguageModelBuilder
	{
		ISequenceStatement ParseInterfaceSnippet(string snippet, bool allowImplicit);

		ISequenceStatement2 CreateSequenceStatementEx(IExprementPosition pos, IEnumerable<IStatement> statements);

		IArrayInitialization CreateArrayInitialisationEx(IExprementPosition pos, IEnumerable<IExpression> expInitvalues);

		IStructureInitialization CreateStructureInitialisationEx(IExprementPosition pos, IEnumerable<IAssignmentExpression> initAssigns);

		IStatement CreatePragmaStatement2(IExprementPosition pos, string pragma);

		ICallExpression2 CreateNonFormalCallExpression(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, IEnumerable<IExpression> inputarguments, IEnumerable<IAssignmentExpression> outputassignments);

		IExpressionStatement CreateNonFormalCallStatement(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, IEnumerable<IExpression> inputarguments, IEnumerable<IAssignmentExpression> outputassignments);

		IPragmaStatement CreatePragmaAttributeStatement(IExprementPosition pos, string attributeName, string attributeValue);

		INullExpression CreateNullExpression(IExprementPosition pos);

		IExpression ParseExpression(IExprementPosition pos, string stExpression, bool allowImplicit);

		IExpression ParseInitialisation(string stExpression, bool allowImplicit);

		ISequenceStatement2 ParseSTSnippet(string stSnippet, bool allowImplicit);

		IExpression ParseExpression(string stExpression, bool allowImplicit);

		ICompiledType ParseType(string stType, bool allowImplicit);

		bool AddTemporaryVariableBeforeCompile(ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype, IExpression expInitValue);

		bool AddTemporaryVariableAfterCompile(ICompileContext comcon, ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype);
	}
}
