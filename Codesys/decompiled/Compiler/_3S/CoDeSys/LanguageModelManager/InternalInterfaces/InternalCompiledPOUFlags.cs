using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	[TypeGuid("{D84E773E-C159-42C6-8F67-07B0DF2B44AB}")]
	public enum InternalCompiledPOUFlags
	{
		[ReleasedEnumMember]
		ContainsTryCatch = 1,
		[ReleasedEnumMember]
		Checked = 2,
		[ReleasedEnumMember]
		ContainsCheckLicense = 4,
		[ReleasedEnumMember]
		LibraryAccessPermitted = 8,
		[ReleasedEnumMember]
		PreserveCompiledLibComments = 0x10,
		[ReleasedEnumMember]
		RemovedFromMemory = 0x20,
		[ReleasedEnumMember]
		ToCheck = 0x40,
		[ReleasedEnumMember]
		RequiresRetypification = 0x80,
		[ReleasedEnumMember]
		SkipParseTreeDuplication = 0x100,
		[ReleasedEnumMember]
		ExcludeParseTreeFromCompiledLib = 0x200,
		[ReleasedEnumMember]
		ContainsVectorOperation = 0x400,
		[ReleasedEnumMember]
		ContainsCallWithOmittedOptionalInput = 0x800,
		[ReleasedEnumMember]
		IsImplicitInitFunction = 0x1000,
		[ReleasedEnumMember]
		ImplicitInitialisationCodeAdded = 0x2000,
		[ReleasedEnumMember]
		TypeCheckDone = 0x4000
	}
}
