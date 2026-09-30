using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IBreakpointSerializable
	{
		short Len { get; set; }

		short TryCatchId { get; set; }

		long PositionCombination { get; set; }

		int[] Successors { get; set; }

		IStepInPosition[] StepInSuccessors { get; set; }

		int[] AssemblySuccessors { get; set; }

		int Offset { get; set; }

		int AreaGPRegister { get; set; }

		int OffsetGPRegister { get; set; }

		int ExceptionHandlingSuccessor { get; set; }
	}
}
