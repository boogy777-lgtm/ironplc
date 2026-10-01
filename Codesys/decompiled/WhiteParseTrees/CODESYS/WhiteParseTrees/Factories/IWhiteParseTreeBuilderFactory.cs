using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeBuilderFactory
	{
		IProgramNameStep CreateProgramBuilder();

		IFunctionNameStep CreateFunctionBuilder();

		IMethodNameStep CreateMethodBuilder();

		IFunctionBlockNameStep CreateFunctionBlockBuilder();

		IUnionNameStep CreateUnionBuilder();

		IPropertyNameStep CreatePropertyBuilder();

		IInterfaceNameStep CreateInterfaceBuilder();

		IVarDeclNameStep CreateVariableDeclarationBuilder();

		ILValueStep CreateAssignmentBuilder();

		ICalleeStep CreateCallBuilder();

		ISwitchCaseStep CreateCaseBuilder();

		ICaseExpressionListStep CreateCaseLabelBuilder();

		IForStartExpressiontStep CreateForBuilder();

		IIfConditionStep CreateIfBuilder();

		IElseIfConditionStep CreateElseIfBuilder();

		IWhiteCase CreateWhiteCase(IWhiteCaseLabelStatement label, IWhiteSequenceStatement controlled);
	}
}
