using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IWhileStatement : IStatement, IExprement
	{
		IExpression Condition { get; }

		IStatement Controlled { get; }
	}
}
