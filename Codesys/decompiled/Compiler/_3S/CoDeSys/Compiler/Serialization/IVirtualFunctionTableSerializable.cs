using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IVirtualFunctionTableSerializable
	{
		IDictionary<int, int> InterfaceIdToOffset { get; set; }

		IDictionary<int, int> OffsetToInterfaceId { get; set; }

		int PointerSize { get; set; }

		IList<IVFTableEntry> _Entries { get; set; }

		_ISignature Signature { get; set; }

		IDataLocation DataLocation { get; set; }
	}
}
