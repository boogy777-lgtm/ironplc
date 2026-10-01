using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public enum InitialValueResult
	{
		[ReleasedEnumMember]
		OK,
		[ReleasedEnumMember]
		NoInitExpression,
		[ReleasedEnumMember]
		AccessPathInvalid,
		[ReleasedEnumMember]
		VariableNotFound,
		[ReleasedEnumMember]
		SourceCodeInvalid
	}
}
