using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope3 : IPrecompileScope2, IPrecompileScope
	{
		ISignature2 FindSignatureGlobal(IExpression expName, out string stNamespace);

		string GetNamespace(string stLibraryId);
	}
}
