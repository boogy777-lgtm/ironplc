using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCreatorService
	{
		ITypeInfo TypeInfo { get; }

		IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		IParser CreateParser(IScanner scanner);

		IParser CreateParser(string stText);

		ILanguageModelBuilder CreateLanguageModelBuilder();
	}
}
