using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IBreakpoint : IBreakpoint3, IBreakpoint2, IBreakpoint
	{
		new int Offset { get; set; }

		IMinimalPosition _Position { get; set; }

		new int ExceptionHandlingSuccessor { get; set; }

		void AddSuccessor(int nSucc);

		void AddSuccessors(params int[] nSucc);

		void AddStepInSuccessor(IStepInPosition sip);
	}
}
