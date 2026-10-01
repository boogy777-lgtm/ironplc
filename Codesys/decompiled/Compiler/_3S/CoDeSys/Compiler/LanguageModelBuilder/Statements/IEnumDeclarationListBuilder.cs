using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IEnumDeclarationListBuilder : IEnumOptionalPosStep, ILmbPositional<IEnumBuilderAddValue>, IEnumBuilderAddValue, IEnumBuilderBaseType
	{
	}
}
