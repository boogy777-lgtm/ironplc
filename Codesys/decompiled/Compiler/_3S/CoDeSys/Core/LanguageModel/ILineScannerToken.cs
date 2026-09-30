using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILineScannerToken
	{
		TokenType Type { get; }

		int LineOffset { get; }

		int Length { get; }
	}
}
