using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpointList2 : IBreakpointList, ICollection, IEnumerable
	{
		IBreakpoint First { get; }
	}
}
