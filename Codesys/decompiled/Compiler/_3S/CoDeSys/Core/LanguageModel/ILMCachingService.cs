using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCachingService
	{
		bool LoadCachedLanguageModel(Stream cache);

		void StoreCachedLanguageModel(Stream cache);
	}
}
