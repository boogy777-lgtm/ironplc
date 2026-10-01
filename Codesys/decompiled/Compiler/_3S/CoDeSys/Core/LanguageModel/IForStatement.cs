using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IForStatement : IStatement, IExprement
	{
		IExpression CounterStart { get; }

		IExpression UpperBound { get; }

		IExpression By { get; }

		IExpression Condition { get; }

		IStatement Controlled { get; }

		IExpression Counter { get; }
	}
}
