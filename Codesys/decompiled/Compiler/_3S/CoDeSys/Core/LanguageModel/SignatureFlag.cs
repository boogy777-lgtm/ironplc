using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{9b5d4c11-b2d9-46c2-bf90-350eefa47fc5}")]
	public enum SignatureFlag : long
	{
		[ReleasedEnumMember]
		None = 0L,
		[ReleasedEnumMember]
		Structure = 1L,
		[ReleasedEnumMember]
		Enum = 2L,
		[ReleasedEnumMember]
		Alias = 4L,
		[ReleasedEnumMember]
		Global = 8L,
		[ReleasedEnumMember]
		External = 0x10L,
		[ReleasedEnumMember]
		NonVirtual = 0x20L,
		[ReleasedEnumMember]
		NoInit = 0x40L,
		[ReleasedEnumMember]
		Action = 0x80L,
		[ReleasedEnumMember]
		Generated = 0x100L,
		[ReleasedEnumMember]
		Temp = 0x200L,
		[ReleasedEnumMember]
		CallersToGenerate = 0x400L,
		[ReleasedEnumMember]
		ToRemoveAfterDownload = 0x800L,
		[ReleasedEnumMember]
		NoCopy = 0x1000L,
		[ReleasedEnumMember]
		NoCompareWithNew = 0x2000L,
		[ReleasedEnumMember]
		Union = 0x4000L,
		[ReleasedEnumMember]
		ImplicitInterfaceUnion = 0x8000L,
		[ReleasedEnumMember]
		ContainsVarConfig = 0x10000L,
		[ReleasedEnumMember]
		InitializeVirtualFunctionTable = 0x20000L,
		[ReleasedEnumMember]
		TimeStampOnly = 0x40000L,
		[ReleasedEnumMember]
		Compiled = 0x80000L,
		[ReleasedEnumMember]
		OnlineChanged = 0x400000L,
		[ReleasedEnumMember]
		ImplicitParamsStruct = 0x800000L,
		[ReleasedEnumMember]
		InhibitOnlineChange = 0x1000000L,
		[ReleasedEnumMember]
		SuperGlobal = 0x2000000L,
		[ReleasedEnumMember]
		Typified = 0x4000000L,
		[ReleasedEnumMember]
		SavePrecompile = 0x8000000L,
		[ReleasedEnumMember]
		Located = 0x10000000L,
		[ReleasedEnumMember]
		Persistent = 0x20000000L,
		[ReleasedEnumMember]
		TopLevel = 0x40000000L,
		[ReleasedEnumMember]
		ContainsLazy = 0x80000000L,
		[ReleasedEnumMember]
		SimulationExternal = 0x100000000L,
		[ReleasedEnumMember]
		FunctionTableChanged = 0x200000000L,
		[ReleasedEnumMember]
		InhibitOnlineChangeOnCodeChanges = 0x400000000L,
		[ReleasedEnumMember]
		NoLink = 0x800000000L,
		[ReleasedEnumMember]
		LinkInSysApp = 0x1000000000L,
		[ReleasedEnumMember]
		ContainsRetain = 0x2000000000L,
		[ReleasedEnumMember]
		ContainsPersistent = 0x4000000000L,
		[ReleasedEnumMember]
		PoolSignature = 0x8000000000L,
		[ReleasedEnumMember]
		Private = 0x10000000000L,
		[ReleasedEnumMember]
		Protected = 0x20000000000L,
		[ReleasedEnumMember]
		Internal = 0x40000000000L,
		[ReleasedEnumMember]
		Final = 0x80000000000L,
		[ReleasedEnumMember]
		InterfaceLibraryObject = 0x100000000000L,
		[ReleasedEnumMember]
		NoIECIdent = 0x200000000000L,
		[ReleasedEnumMember]
		SystemNamespaceForced = 0x400000000000L,
		[ReleasedEnumMember]
		Abstract = 0x800000000000L,
		[ReleasedEnumMember]
		RawSTProperty = 0x1000000000000L,
		[ReleasedEnumMember]
		RawSTTransition = 0x2000000000000L
	}
}
