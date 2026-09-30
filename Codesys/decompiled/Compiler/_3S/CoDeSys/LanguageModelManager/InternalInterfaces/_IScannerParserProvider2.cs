using System;
using _3S.CoDeSys.Core.Components;
using CODESYS.Parser;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IScannerParserProvider2 : _IScannerParserProvider
	{
		IScannerService GetScannerService(Version version);

		IParserService GetParserService(Version version);
	}
}
