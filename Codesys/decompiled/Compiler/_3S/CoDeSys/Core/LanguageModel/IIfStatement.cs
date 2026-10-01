using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIfStatement : IStatement, IExprement
	{
		IExpression Condition { get; }

		IStatement IfThen { get; }

		IStatement IfElse { get; }

		IElseIf[] ElseIf { get; }
	}
}
