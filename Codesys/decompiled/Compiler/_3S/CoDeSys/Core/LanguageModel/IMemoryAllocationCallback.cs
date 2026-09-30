using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemoryAllocationCallback
	{
		void OnBeforeCodeAllocation(List<ICompiledPOU4> cpous, ICompileContext6 comcon);

		DataSegmentFlags GetDataSegmentFlagForCode(ICompiledPOU4 cpou, ISignature3 sign, ICompileContext6 comcon, DataSegmentFlags dsDefault);

		DataSegmentFlags GetDataSegmentFlagForVariable(IVariable3 var, ISignature3 sign, ICompileContext6 comcon, DataSegmentFlags dsDefault);

		DataSegmentFlags GetDataSegmentFlagForGVLSubsequent(ISignature3 sign, ICompileContext6 comcon, DataSegmentFlags dsDefault);

		DataSegmentFlags GetDataSegmentFlagForFunctionPointer(ISignature3 sign, ICompileContext6 comcon, DataSegmentFlags dsDefault);

		DataSegmentFlags GetDataSegmentFlagForVirtualFunctionTable(ISignature3 sign, ICompileContext6 comcon, DataSegmentFlags dsDefault);
	}
}
