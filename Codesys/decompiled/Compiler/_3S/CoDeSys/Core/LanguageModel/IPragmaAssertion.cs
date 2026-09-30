using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPragmaAssertion : IStatement, IExprement
	{
		IExpression ConditionExpression { get; }

		string ErrorOutput { get; }
	}
}
