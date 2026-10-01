using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public enum ESignatureRelationship
	{
		[ReleasedEnumMember]
		Unknown,
		[ReleasedEnumMember]
		InitialSignature,
		[ReleasedEnumMember]
		BaseImplementation,
		[ReleasedEnumMember]
		Inheritance,
		[ReleasedEnumMember]
		ImplementationOfInterface
	}
}
