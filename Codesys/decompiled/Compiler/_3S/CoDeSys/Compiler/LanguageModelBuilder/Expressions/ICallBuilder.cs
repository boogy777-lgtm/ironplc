using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions
{
	[ReleasedInterface]
	public interface ICallBuilder : ICalleeBuilderOptionalPosStep, ILmbPositional<ICalleeBuilder>, ICalleeBuilder
	{
	}
}
