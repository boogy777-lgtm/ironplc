using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IAdditionalAttributeProvider
	{
		string ProvideAdditionalAttribute(ISignature2 signFB, ISignature2 signMethod, IPreCompileContext3 precomSource);
	}
}
