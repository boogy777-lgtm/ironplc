using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IGVLBuilder : IHasVarDeclaration, IHasAttributes
	{
		void SetGlobalInitSlot(string stGlobalInitSlot);

		void SetSignatureFlag(SignatureFlag signatureFlag);

		void AddErrorMessage(string stErrorMessage);
	}
}
