using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakpointList : ICollection, IEnumerable
	{
		IBreakpoint this[int nIndex] { get; }

		IBreakpoint FindBySourcePosition(ISourcePosition sourcepos);

		IBreakpoint GetByCodePosition(int nCodeOffset);

		IBreakpoint GetByStepOutPosition(int nCodeOffset, out IStepInPosition stepinpos);
	}
}
