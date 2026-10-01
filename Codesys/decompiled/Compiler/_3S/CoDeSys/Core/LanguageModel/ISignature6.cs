using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISignature6 : ISignature5, ISignature4, ISignature3, ISignature2, ISignature
	{
		bool IsCompiledLibraryObject { get; }

		int PrecompileId { get; }

		int PrecompileParentId { get; }

		int PrecompileBaseSignatureId { get; }

		IEnumerable<int> PrecompileDeclarerIds { get; }

		IEnumerable<int> PrecompileCallerIds { get; }

		IEnumerable<int> PrecompileCalleeIds { get; }

		IEnumerable<int> PrecompileInterfaceSignatureIds { get; }

		IVariable4 GetVariableForPrecompileId(int precompileId);
	}
}
