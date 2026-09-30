using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IGlobalScopeExpression : IExpression2, IExpression, IExprement
	{
		IExpression Base { get; }
	}
}
