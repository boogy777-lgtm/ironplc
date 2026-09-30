using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager29 : ILanguageModelManager28, ILanguageModelManager27, ILanguageModelManager26, ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		[Obsolete("GetAreaSizeAfterShrinking should no longer be used. Use GetDownloadInfo instead. IDownloadInfo.Areas contains already shrinked area sizes.")]
		ulong[] GetAreaSizesAfterShrinking(Guid appGuid);

		void EnablePrecompileChecksInNoUIMode();

		void DisablePrecompileChecksInNoUIMode();
	}
}
