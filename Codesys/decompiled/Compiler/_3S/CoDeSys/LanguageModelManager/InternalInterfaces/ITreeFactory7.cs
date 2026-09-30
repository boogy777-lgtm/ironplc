using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory7 : ITreeFactory6, ITreeFactory5, ITreeFactory4, ITreeFactory3, ITreeFactory2
	{
		_IImplicitCodeSectionPragma CreateImplicitCodeSectionPragma(bool bOn, string stText);

		_ILocalSignatureIdPragma CreateLocalSignatureIdPragma(int nId, string stText);
	}
}
