using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions
{
	[ReleasedInterface]
	public interface IOperatorBuilder : IOperatorOptionalPosStep, ILmbPositional<IOperatorLhsBuilder>, IOperatorLhsBuilder
	{
	}
}
