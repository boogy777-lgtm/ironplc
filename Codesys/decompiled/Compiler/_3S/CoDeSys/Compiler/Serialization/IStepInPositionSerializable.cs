using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IStepInPositionSerializable
	{
		KindOfCall KindOfCall { get; set; }

		int SignatureId { get; set; }

		IBreakpoint StepInBreakpoint { get; set; }
	}
}
