using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompoAccessExpression : IExpression2, IExpression, IExprement
	{
		IExpression Left { get; }

		IExpression Right { get; }
	}
}
