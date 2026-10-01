using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStructuredLanguageModelSnippetProvider
	{
		ISequenceStatement GetStructuredDeclarationSnippet(ILanguageModelBuilder builder, out Operator kind, out string name);

		ISequenceStatement GetStructuredImplementationSnippet(ILanguageModelBuilder builder);
	}
}
