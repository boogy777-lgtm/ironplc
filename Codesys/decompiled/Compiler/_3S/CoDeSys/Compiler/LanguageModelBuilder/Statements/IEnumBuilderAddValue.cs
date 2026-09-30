using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IEnumBuilderAddValue : IEnumBuilderBaseType
	{
		IEnumBuilderAddValue Enumeration(string stName, IExpression expInit);

		IEnumBuilderAddValue Enumeration(IEnumDeclarationStatement enumDeclarationStatement);
	}
}
