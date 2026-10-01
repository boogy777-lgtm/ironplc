using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelBuilder7 : _ILanguageModelBuilder6, _ILanguageModelBuilder5, _ILanguageModelBuilder4, _ILanguageModelBuilder3, _ILanguageModelBuilder2, _ILanguageModelBuilder, ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		_IImplicitCodeSectionPragma CreateImplicitCodeSectionPragma(IToken token, string stText, bool bOn);

		_ILocalSignatureIdPragma CreateLocalSignatureIdPragma(int nId, string stText);
	}
}
