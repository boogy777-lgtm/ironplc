using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpointCollection : IBreakpointList2, IBreakpointList, ICollection, IEnumerable
	{
		IBreakpoint GetStepIntoBreakpointBySourcePosition(ISourcePosition sourcepos);
	}
}
