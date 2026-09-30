using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISignature2 : _ISignature, ISignature7, ISignature6, ISignature5, ISignature4, ISignature3, ISignature2, ISignature
	{
		_IVirtualFunctionTable _VirtualFunctionTable { get; }

		new int Id { get; set; }

		SignatureFlag Flags { get; set; }

		SignatureFlagInternal InternalFlags { get; set; }

		void ReplaceSubSignature(_ISignature signOld, _ISignature signNew);

		bool AddGreenVariable(_IVariable var);

		bool ReplaceVariablesByGreenVariables(IEnumerable<_IVariable> greenvars);
	}
}
