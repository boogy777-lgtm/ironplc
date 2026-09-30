using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager23 : ILanguageModelManager22, ILanguageModelManager21
	{
		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport);
	}
}
