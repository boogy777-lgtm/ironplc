using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpoint4 : IBreakpoint3, IBreakpoint2, IBreakpoint
	{
		short TryCatchId { get; }
	}
}
