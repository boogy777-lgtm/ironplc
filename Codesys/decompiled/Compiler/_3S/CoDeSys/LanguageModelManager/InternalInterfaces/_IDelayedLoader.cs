using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDelayedLoader
	{
		void CompleteLanguageModel(IProgressCallback callback);
	}
}
