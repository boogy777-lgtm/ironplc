using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext2 : ICompileContext, ICompileContextCommon
	{
		IBreakpoint FindBreakpointByCodePosition(ushort usArea, uint uiOffset, bool bBackward, out ICompiledPOU cpou);
	}
}
