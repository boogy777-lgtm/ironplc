using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParser3 : IParser2, IParser
	{
		IStatement ParseVariableDeclarationList();

		ISignature ParseGlobalVarlist(string stName);

		ISignature ParseInterface();
	}
}
