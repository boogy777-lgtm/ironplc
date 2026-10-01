using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILineScanner
	{
		ILineScannerContext ScanLine(IScanner scanner, string stLine, ILineScannerContext prevContext, bool bReturnTokens, out ILineScannerToken[] tokens);
	}
}
