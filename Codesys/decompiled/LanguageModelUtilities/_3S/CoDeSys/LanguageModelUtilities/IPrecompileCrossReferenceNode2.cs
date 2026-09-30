using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPrecompileCrossReferenceNode2 : IPrecompileCrossReferenceNode, ICrossReferenceNode, IMessage
	{
		ISignature6 ReferencedSignature { get; }
	}
}
