using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPragmaIfStatement : IStatement, IExprement
	{
		IExpression ConditionExpression { get; }

		IStatement IfThenStatement { get; }

		IStatement IfElseStatement { get; }

		IElseIf[] ElseIfs { get; }
	}
}
