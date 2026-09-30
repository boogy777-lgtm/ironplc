using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner2 : IScanner
	{
		bool IncludePositionPragmas { get; set; }
	}
}
