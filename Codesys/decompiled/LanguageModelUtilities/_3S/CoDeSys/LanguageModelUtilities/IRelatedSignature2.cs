using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRelatedSignature2 : IRelatedSignature
	{
		ESignatureRelationship SignatureRelationship { get; }
	}
}
