using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IPragmaScannerFactory
	{
		IPragmaScanner Create(string stPragma);

		IPragmaScanner Create(_IScanner5 scanner);
	}
}
