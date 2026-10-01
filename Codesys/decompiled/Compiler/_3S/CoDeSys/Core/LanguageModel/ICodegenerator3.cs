using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator3 : ICodegenerator2, ICodegenerator
	{
		int RegisterSize { get; }

		bool GetProperty(CodegeneratorProperties cgpProperty);
	}
}
