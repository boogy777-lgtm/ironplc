using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INewExpression : IExpression2, IExpression, IExprement
	{
		ICompiledType TypeToCreate { get; }

		IExpression Count { get; }
	}
}
