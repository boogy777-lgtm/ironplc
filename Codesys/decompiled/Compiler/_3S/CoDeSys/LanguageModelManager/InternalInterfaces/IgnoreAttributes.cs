using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	public enum IgnoreAttributes : uint
	{
		[ReleasedEnumMember]
		Comment = 1u,
		[ReleasedEnumMember]
		DocuComment = 2u,
		[ReleasedEnumMember]
		MessageGuid = 4u,
		[ReleasedEnumMember]
		SignatureCRC = 8u,
		[ReleasedEnumMember]
		VarLenArrayOriginalScope = 0x10u,
		[ReleasedEnumMember]
		AllForVariable = 7u,
		[ReleasedEnumMember]
		AllForSignature = 0xFu
	}
}
