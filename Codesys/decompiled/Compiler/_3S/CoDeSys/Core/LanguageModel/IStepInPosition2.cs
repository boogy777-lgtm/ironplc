using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStepInPosition2 : IStepInPosition
	{
		IBreakpoint StepInBreakpoint { get; set; }
	}
}
