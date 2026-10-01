using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStructuredLanguageModelProviderDelayedSupport
	{
		ILanguageModel GetStructuredLanguageModel(ILanguageModelBuilder lmbuilder, bool forceCompleteLanguageModel);
	}
}
