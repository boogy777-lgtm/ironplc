using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITransitionUserCodeAnalyzationResult
	{
		string StringForAnalyzation { get; }

		bool HasExplicitTransitionAssignment { get; }

		IExpression SingleExpression { get; }

		int CountStatements { get; }
	}
}
