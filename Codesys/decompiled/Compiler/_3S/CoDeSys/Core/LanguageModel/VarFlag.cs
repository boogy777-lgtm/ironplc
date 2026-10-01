using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{1bfe400e-6f3f-4faa-a8db-db81cd2849ba}")]
	public enum VarFlag : long
	{
		[ReleasedEnumMember]
		None = 0L,
		[ReleasedEnumMember]
		Local = 1L,
		[ReleasedEnumMember]
		Input = 2L,
		[ReleasedEnumMember]
		Output = 4L,
		[ReleasedEnumMember]
		Inout = 8L,
		[ReleasedEnumMember]
		External = 0x10L,
		[ReleasedEnumMember]
		ReplacedConstant = 0x20L,
		[ReleasedEnumMember]
		Constant = 0x40L,
		[ReleasedEnumMember]
		Enum = 0x80L,
		[ReleasedEnumMember]
		Alias = 0x100L,
		[ReleasedEnumMember]
		Structure = 0x200L,
		[ReleasedEnumMember]
		Retain = 0x400L,
		[ReleasedEnumMember]
		Persistent = 0x800L,
		[ReleasedEnumMember]
		VarConfig = 0x1000L,
		[ReleasedEnumMember]
		Global = 0x2000L,
		[ReleasedEnumMember]
		VarAccess = 0x4000L,
		[ReleasedEnumMember]
		IsCompiled = 0x8000L,
		[ReleasedEnumMember]
		RelativeStack = 0x10000L,
		[ReleasedEnumMember]
		RelativeInstance = 0x20000L,
		[ReleasedEnumMember]
		Absolut = 0x40000L,
		[ReleasedEnumMember]
		NoInit = 0x80000L,
		[ReleasedEnumMember]
		Implicit = 0x100000L,
		[ReleasedEnumMember]
		Temp = 0x200000L,
		[ReleasedEnumMember]
		LocationChanged = 0x400000L,
		[ReleasedEnumMember]
		OnlChangeCopy = 0x800000L,
		[ReleasedEnumMember]
		OnlChangeInit = 0x1000000L,
		[ReleasedEnumMember]
		NoCopy = 0x2000000L,
		[ReleasedEnumMember]
		Union = 0x4000000L,
		[ReleasedEnumMember]
		Static = 0x8000000L,
		[ReleasedEnumMember]
		Lazy = 0x10000000L,
		[ReleasedEnumMember]
		OnlChangeNoExit = 0x20000000L,
		[ReleasedEnumMember]
		ImplicitParamsStruct = 0x40000000L,
		[ReleasedEnumMember]
		OnlChangeVFInit = 0x80000000L,
		[ReleasedEnumMember]
		LocalPersistent = 0x100000000L,
		[ReleasedEnumMember]
		Initialized = 0x200000000L,
		[ReleasedEnumMember]
		OnlChangeExit = 0x400000000L,
		[ReleasedEnumMember]
		Internal = 0x800000000L,
		[ReleasedEnumMember]
		NoIECIdent = 0x1000000000L,
		[ReleasedEnumMember]
		AllocateInInstance = 0x2000000000L,
		[ReleasedEnumMember]
		OnlChangeReInit = 0x4000000000L,
		[ReleasedEnumMember]
		Typified = 0x8000000000L,
		[ReleasedEnumMember]
		TaskLocal = 0x10000000000L,
		[ReleasedEnumMember]
		Generic = 0x20000000000L,
		[ReleasedEnumMember]
		GenericConstant = 0x40000000000L
	}
}
