using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IPouDeclarationBuilderExtends
	{
		IPouDeclarationBuilderOptionalImplements Extends(params IExpression[] expressions);
	}
}
