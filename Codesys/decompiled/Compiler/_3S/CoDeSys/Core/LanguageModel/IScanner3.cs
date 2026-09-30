using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner3 : IScanner2, IScanner
	{
		bool SupportUnicodeIdentifiers { get; set; }
	}
}
