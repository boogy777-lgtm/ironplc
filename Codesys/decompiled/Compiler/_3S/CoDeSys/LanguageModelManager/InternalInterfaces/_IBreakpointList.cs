using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IBreakpointList : IBreakpointCollection, IBreakpointList2, IBreakpointList, ICollection, IEnumerable
	{
		int FirstIndex { get; set; }

		_IBreakpoint FindNearestByCodePosition(int nCodeOffset, bool bBackward);

		void UpdateSourcePositions(IDictionary<IMinimalPosition, IMinimalPosition> lSourcePosMap);

		int Add(ref _IBreakpoint bp);

		void Remove(int nIndex);

		void ChangeSourcePos(IBreakpoint bp, long lPosition);

		string Dump(ICompileContext comcon);
	}
}
