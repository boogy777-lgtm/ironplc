using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IMethodImplementationService
	{
		IEnumerable<IAbstractMethodDescription> GetAbstractMethodsToImplement(ISignature2 sign, IPreCompileContext comcon);

		IEnumerable<IAbstractMethodDescription> GetInterfaceMethodsToImplement(ISignature2 sign, IPreCompileContext comcon);

		string GetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest);

		string CreateTextualInterface(ISignature2 signFB, ISignature2 signMethod, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, IAdditionalAttributeProvider additionalAttributeProvider);
	}
}
