using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileStorageSizeEstimatorService
	{
		bool CalculateVariableSizes(ILMPreCompileSet preCompileSet, IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes);

		int CalculateTypeSize(ILMPreCompileSet preCompileSet, ISignature sign, IType type);

		int CalculateSignatureSize(ILMPreCompileSet preCompileSet, ISignature3 sign);

		int CalculatePointerSize(Guid guidApplication);

		int GetGranularity(ILMPreCompileSet precom, ICompiledType type);
	}
}
