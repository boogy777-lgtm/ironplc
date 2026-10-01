using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableReference : IExpression2, IExpression, IExprement
	{
		IExpression Instance { get; }
	}
}
