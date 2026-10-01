using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVirtualFunctionTable : IVirtualFunctionTable2, IVirtualFunctionTable
	{
		new IDataLocation DataLocation { get; set; }

		_ISignature Signature { get; set; }

		int this[string strName] { get; }

		IList<IVFTableEntry> _Entries { get; }

		bool IsEqual(_IVirtualFunctionTable vftableIn);

		IDictionary<int, int> GetOffsetInterfaceMap();

		int GetInterfaceOffset(string stMethodName, IScope scope, _ICompileContext comcon);
	}
}
