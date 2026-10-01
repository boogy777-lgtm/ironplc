using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStepInPosition
	{
		int SignatureId { get; }

		KindOfCall KindOfCall { get; }

		IBreakpoint StepOutBreakpoint { get; }
	}
}
