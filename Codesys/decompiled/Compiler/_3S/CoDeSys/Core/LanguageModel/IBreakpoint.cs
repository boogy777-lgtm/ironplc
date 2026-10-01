using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpoint
	{
		int Offset { get; set; }

		ISourcePosition Position { get; }

		int[] Successors { get; }

		IStepInPosition[] StepInSuccessors { get; }

		int[] AssemblySuccessors { get; }

		void AddAssemblySuccessor(int nSuccessorOffset);
	}
}
