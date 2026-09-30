using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ILocalCodeWriter
	{
		IPreCompileContext LocalContext { get; }

		string GetTypeText(IType type, IPreCompileContext precomLib);

		string GetSignatureInterface(ISignature sign);
	}
}
