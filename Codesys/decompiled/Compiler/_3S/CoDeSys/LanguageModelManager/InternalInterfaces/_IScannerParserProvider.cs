using _3S.CoDeSys.Core.Components;
using CODESYS.Parser;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IScannerParserProvider
	{
		IScannerService ScannerServiceToUseInternal();

		IParserService ParserServiceToUseInternal();
	}
}
