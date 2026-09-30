using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStructuredLanguageModelProvider
	{
		ILanguageModel GetStructuredLanguageModel(ILanguageModelBuilder lmbuilder);
	}
}
