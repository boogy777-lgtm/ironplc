using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IPreCompileSizeCalculator
	{
		bool CalculateVariableSizes(_IPreCompileContext precom, IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes);

		int CalculateTypeSize(_IPreCompileContext precom, ISignature sign, IType type, IRecursionGuard recursionGuard);

		int CalculateSignatureSize(_IPreCompileContext precom, ISignature3 sign, IRecursionGuard recursionGuard);
	}
}
