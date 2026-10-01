using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITaskCrossref
	{
		byte TaskId { get; }

		ICrossReference CrossRef { get; }

		int SignatureId { get; }
	}
}
