using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAssignmentExpression : IExpression2, IExpression, IExprement
	{
		IExpression LValue { get; }

		IExpression RValue { get; }

		Operator KindOf { get; }
	}
}
