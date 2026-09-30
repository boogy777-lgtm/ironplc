using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EPouSetChange : ulong
	{
		[ReleasedEnumMember]
		Undefined = 0uL,
		[ReleasedEnumMember]
		NewCheckFunctionInserted = 1uL,
		[ReleasedEnumMember]
		InhibitOnlineChange = 2uL,
		[ReleasedEnumMember]
		InterfacesRemoved = 4uL,
		[ReleasedEnumMember]
		InterfaceSequenceChanged = 8uL,
		[ReleasedEnumMember]
		BaseExpressionChanged = 0x10uL,
		[ReleasedEnumMember]
		SignatureChangedByChecksumAttribute = 0x20uL,
		[ReleasedEnumMember]
		CompileOptionsChanged = 0x40uL,
		[ReleasedEnumMember]
		ParentContextNull = 0x80uL,
		[ReleasedEnumMember]
		ParentContextChanged = 0x100uL,
		[ReleasedEnumMember]
		MemorySettingsChanged = 0x200uL,
		[ReleasedEnumMember]
		TaskLocalGvl = 0x400uL,
		[ReleasedEnumMember]
		GlobalInitInCycle = 0x800uL,
		[ReleasedEnumMember]
		TaskListChanged = 0x1000uL,
		[ReleasedEnumMember]
		DeviceApplication = 0x2000uL,
		[ReleasedEnumMember]
		BlobInitChanged = 0x4000uL,
		[ReleasedEnumMember]
		ExternalSignatureFlagChanged = 0x8000uL,
		[ReleasedEnumMember]
		NameExpressionChanged = 0x10000uL,
		[ReleasedEnumMember]
		InhibitOnlineChangeOnCodeChanges = 0x20000uL,
		[ReleasedEnumMember]
		CheckFunction = 0x40000uL,
		[ReleasedEnumMember]
		PrecomTypeMayLeaveDeadRefs = 0x80000uL,
		[ReleasedEnumMember]
		NumberOfSubSignaturesChanged = 0x100000000uL,
		[ReleasedEnumMember]
		InterfaceAdded = 0x200000000uL,
		[ReleasedEnumMember]
		CodeChanged = 0x400000000uL,
		[ReleasedEnumMember]
		DefineChanged = 0x800000000uL,
		[ReleasedEnumMember]
		GlobalErrorsChanged = 0x1000000000uL,
		[ReleasedEnumMember]
		ReferencedLibrariesChanged = 0x2000000000uL,
		[ReleasedEnumMember]
		ParameterTablesChanged = 0x4000000000uL,
		[ReleasedEnumMember]
		InterfaceDeleted = 0x8000000000uL,
		[ReleasedEnumMember]
		InterfaceChanged = 0x10000000000uL,
		[ReleasedEnumMember]
		InterfaceChangedExplicitly = 0x20000000000uL,
		[ReleasedEnumMember]
		VariableInserted = 0x40000000000uL,
		[ReleasedEnumMember]
		VariableChanged = 0x80000000000uL,
		[ReleasedEnumMember]
		VariableDeleted = 0x100000000000uL,
		[ReleasedEnumMember]
		AttributesChanged = 0x200000000000uL,
		[ReleasedEnumMember]
		ImplicitObject = 0x1000000000000000uL,
		[ReleasedEnumMember]
		ProhibitsOnlineChangeMask = 0xFFFFFFFFuL
	}
}
