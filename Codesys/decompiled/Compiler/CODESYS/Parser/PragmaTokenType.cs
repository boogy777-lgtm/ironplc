using _3S.CoDeSys.Core.Components;

namespace CODESYS.Parser
{
	public enum PragmaTokenType
	{
		[ReleasedEnumMember]
		None,
		[ReleasedEnumMember]
		Identifier,
		[ReleasedEnumMember]
		Integer,
		[ReleasedEnumMember]
		Operator,
		[ReleasedEnumMember]
		SingleByteString,
		[ReleasedEnumMember]
		Error,
		[ReleasedEnumMember]
		End
	}
}
