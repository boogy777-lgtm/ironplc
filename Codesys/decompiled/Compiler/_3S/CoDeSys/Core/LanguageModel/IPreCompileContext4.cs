using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext4 : IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		bool SystemApplication { get; }

		string SystemApplicationName { get; }

		bool SupportSystemApplication { get; }

		IPrecompileLibInfo[] ReferencedLibraries { get; }

		ILibraryPlaceholder[] ReferencedPlaceholders { get; }

		bool CalculateVariableSizes(List<ISignature> signaturelist, List<IVariable> varlist, out List<int> sizes);

		int CalculateTypeSize(ISignature sign, IType type);

		int CalculateSignatureSize(ISignature3 sign);

		ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(Guid guidApplication);
	}
}
