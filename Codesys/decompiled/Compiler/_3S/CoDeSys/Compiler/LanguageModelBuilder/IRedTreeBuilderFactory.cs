using _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder
{
	[ReleasedInterface]
	public interface IRedTreeBuilderFactory
	{
		ICalleeBuilderOptionalPosStep CallBuilder { get; }

		IOperatorOptionalPosStep OperatorBuilder { get; }

		IIfOptionalPosStep IfBuilder { get; }

		IForOptionalPosStep ForBuilder { get; }

		ICaseOptionalPosStep CaseBuilder { get; }

		IWhileOptionalPosStep WhileBuilder { get; }

		ITypeOptionalPosStep TypeDeclarationBuilder { get; }

		IElseIfOptionalPosStep ElseIfBuilder { get; }

		IPouDeclarationBuilderOptionalPosStep PouDeclarationBuilder { get; }

		IEnumOptionalPosStep EnumDeclarationListBuilder { get; }

		IVariableDeclarationListOptionalPosStep VariableDeclarationListBuilder { get; }
	}
}
