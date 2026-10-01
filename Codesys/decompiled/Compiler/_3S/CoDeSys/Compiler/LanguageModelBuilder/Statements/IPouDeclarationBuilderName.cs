using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IPouDeclarationBuilderName
	{
		IPouDeclarationBuilderOptionalExtends Name(string name);
	}
}
