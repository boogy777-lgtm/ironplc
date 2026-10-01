using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IAbstractMethodDescription
	{
		ISignature Signature { get; }

		ISignature ParentSignature { get; }

		IPreCompileContext PreCompileContext { get; }
	}
}
