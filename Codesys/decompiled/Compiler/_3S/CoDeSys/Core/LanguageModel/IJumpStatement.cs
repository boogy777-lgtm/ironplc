using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IJumpStatement : IStatement, IExprement
	{
		IExpression Condition { get; }

		string Label { get; }
	}
}
