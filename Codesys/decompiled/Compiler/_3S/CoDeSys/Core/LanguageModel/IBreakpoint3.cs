using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpoint3 : IBreakpoint2, IBreakpoint
	{
		int ExceptionHandlingSuccessor { get; }
	}
}
