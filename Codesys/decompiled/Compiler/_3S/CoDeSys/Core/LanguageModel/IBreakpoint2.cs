using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpoint2 : IBreakpoint
	{
		int AreaGPRegister { get; set; }

		int OffsetGPRegister { get; set; }
	}
}
