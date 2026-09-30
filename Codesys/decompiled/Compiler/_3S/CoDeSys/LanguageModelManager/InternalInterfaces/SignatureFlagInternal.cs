using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	public enum SignatureFlagInternal : long
	{
		[ReleasedEnumMember]
		None = 0L,
		[ReleasedEnumMember]
		Checked = 1L,
		[ReleasedEnumMember]
		PrecompiledLib = 2L,
		[ReleasedEnumMember]
		ContainsInstanceVars = 4L,
		[ReleasedEnumMember]
		ReadyToCheck = 8L,
		[ReleasedEnumMember]
		ExplicitInitExitHandling = 0x10L,
		[ReleasedEnumMember]
		CallWithinGlobalInitExit = 0x20L,
		[ReleasedEnumMember]
		VersionFreeLibrary = 0x40L,
		[ReleasedEnumMember]
		StructureContainsFBInstances = 0x80L,
		[ReleasedEnumMember]
		PreserveCompiledLibComments = 0x100L,
		[ReleasedEnumMember]
		InitialValuesChecked = 0x200L,
		[ReleasedEnumMember]
		VariablesTypified = 0x400L,
		[ReleasedEnumMember]
		ContainsImplicitReferenceType = 0x800L,
		[ReleasedEnumMember]
		ForceOnlineChangeCopy = 0x1000L,
		[ReleasedEnumMember]
		OnlineChangePartialInit = 0x2000L,
		[ReleasedEnumMember]
		ContainsAccessToCurrentTask = 0x4000L,
		[ReleasedEnumMember]
		CopyFBInitVarTypesFromBaseSign = 0x8000L,
		[ReleasedEnumMember]
		OptionalInputs = 0x10000L,
		[ReleasedEnumMember]
		ContainsGenericConstants = 0x20000L,
		[ReleasedEnumMember]
		ContainsGenericInstanceVar = 0x40000L,
		[ReleasedEnumMember]
		ContainsCallWithOmittedOptionalInput = 0x80000L,
		[ReleasedEnumMember]
		Overloaded = 0x100000L,
		[ReleasedEnumMember]
		Overloading = 0x200000L
	}
}
