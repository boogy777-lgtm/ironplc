using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator2 : ICodegenerator
	{
		bool SupportSetNextStatement { get; }
	}
}
