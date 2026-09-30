using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator11 : ICodegenerator10, ICodegenerator9, ICodegenerator8, ICodegenerator7, ICodegenerator6, ICodegenerator5, ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		bool TryGetIntProperty(CodegeneratorIntProperty cgpProperty, out int value);
	}
}
