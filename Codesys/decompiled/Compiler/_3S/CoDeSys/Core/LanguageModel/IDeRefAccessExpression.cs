using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeRefAccessExpression : IExpression2, IExpression, IExprement
	{
		IExpression Base { get; }
	}
}
