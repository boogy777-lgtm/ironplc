using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStructuredImplicitDeclarationSnippetProvider
	{
		ISequenceStatement GetStructuredImplicitDeclarationSnippet(ILanguageModelBuilder builder);
	}
}
