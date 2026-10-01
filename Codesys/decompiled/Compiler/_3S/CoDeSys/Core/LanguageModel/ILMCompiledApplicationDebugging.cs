using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationDebugging
	{
		IBreakpoint GetBreakpointByCodePosition(ushort usArea, uint uiOffset, out ICompiledPOU cpou);

		IBreakpoint FindBreakpointByCodePosition(ushort usArea, uint uiOffset, bool bBackward, out ICompiledPOU cpou);

		ICompiledPOU GetPOUByCodePosition(ushort usArea, uint uiOffset);

		ICompiledPOU GetTaskSuccessor(ICompiledPOU cpouPredecessor, ISignature signTaskPOU);

		IVariable AddWatchVariable(string stName, ICompiledType type);

		IVariable AddWatchVariable(string stName, ICompiledType type, IDataLocation requestedLocation);

		IVariable GetWatchVariable(string stName);

		void RemoveWatchVariables();
	}
}
