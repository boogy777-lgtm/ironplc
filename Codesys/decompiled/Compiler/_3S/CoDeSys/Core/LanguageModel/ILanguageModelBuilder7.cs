using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder7 : ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(bool bRestore, string stId);
	}
}
