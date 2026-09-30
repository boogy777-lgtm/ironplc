using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPreCompileContext2 : _IPreCompileContext, IPreCompileContext14, IPreCompileContext13, IPreCompileContext12, IPreCompileContext11, IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		new bool SavedWithUnicodeIdentifiers { get; set; }

		IEnumerable<int> GetSignaturesForGlobalVarName(string name);

		int CalculateSignatureSize(ISignature3 sign, IRecursionGuard recursionGuard);

		int CalculateTypeSize(ISignature sign, IType type, IRecursionGuard recursionGuard);

		bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes);

		void AddGreenSignature(_ISignature sign, bool bNotify);

		void AddGreenCompiledPOU(_ICompiledPOU cpou, bool bNotify);
	}
}
