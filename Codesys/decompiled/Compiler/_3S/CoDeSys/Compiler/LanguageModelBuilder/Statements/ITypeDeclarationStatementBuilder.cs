using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface ITypeDeclarationStatementBuilder : ITypeOptionalPosStep, ILmbPositional<ITypeBuilderChoose>, ITypeBuilderChoose, ITypeBuilderAlias, ITypeBuilderName
	{
	}
}
