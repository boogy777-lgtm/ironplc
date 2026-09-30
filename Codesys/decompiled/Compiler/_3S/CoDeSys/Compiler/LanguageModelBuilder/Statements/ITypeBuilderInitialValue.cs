using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface ITypeBuilderInitialValue
	{
		ITypeBuilderFlag InitialValue(IExpression value);
	}
}
