using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScopeWithAliasService
	{
		ISignature DetermineAliasBaseSignature(ISignature sign, out IPrecompileScope7 foundScope);

		ICompiledType DetermineAliasBaseType(ICompiledType type, out IPrecompileScope7 foundScope);
	}
}
