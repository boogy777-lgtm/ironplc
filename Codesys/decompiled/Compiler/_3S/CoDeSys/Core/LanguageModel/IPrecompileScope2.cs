using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope2 : IPrecompileScope
	{
		ISignature FindSignatureGlobal(IExpression qne);
	}
}
