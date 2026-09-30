using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IEvaluationContext2 : IEvaluationContext
	{
		int ProjectHandle { get; }

		int AttractingProjectHandle { get; }
	}
}
