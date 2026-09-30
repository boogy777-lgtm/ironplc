using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IFlowVarRef : IVarRef2, IVarRef
	{
		IBreakpoint Breakpoint { get; }

		IBreakpoint[] BreakpointsValue { get; }

		IBreakpoint[] BreakpointsReached { get; }

		ICompiledPOU CompiledPOU { get; }

		string InstancePath { get; }

		bool Reached { get; set; }

		bool ReadAccess { get; }
	}
}
