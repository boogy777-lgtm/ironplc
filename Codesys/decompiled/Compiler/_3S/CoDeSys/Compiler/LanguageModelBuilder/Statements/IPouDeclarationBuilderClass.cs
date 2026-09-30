using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IPouDeclarationBuilderClass
	{
		IPouDeclarationBuilderOptionalAccess AsProgram();

		IPouDeclarationBuilderOptionalAccess AsFunction();

		IPouDeclarationBuilderOptionalAccess AsFunctionBlock();

		IPouDeclarationBuilderOptionalAccess AsInterface();

		IPouDeclarationBuilderOptionalAccess AsMethod();

		IPouDeclarationBuilderOptionalAccess AsAction();
	}
}
