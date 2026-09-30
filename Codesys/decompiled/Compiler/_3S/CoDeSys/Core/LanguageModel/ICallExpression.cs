using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICallExpression : IExpression2, IExpression, IExprement
	{
		IExpression Callee { get; }

		IExpression Condition { get; }

		IAssignmentExpression[] InputAssigns { get; }

		IAssignmentExpression[] OutputAssigns { get; }
	}
}
