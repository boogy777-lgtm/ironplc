using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{ffb74c62-0dab-4b81-a769-562edfcba448}")]
	public enum CompiledPOUFlags
	{
		[ReleasedEnumMember]
		TopLevel = 1,
		[ReleasedEnumMember]
		PreCompiled = 2,
		[ReleasedEnumMember]
		Typified = 4,
		[ReleasedEnumMember]
		ToCompile = 8,
		[ReleasedEnumMember]
		ToRemoveAfterDownload = 0x10,
		[ReleasedEnumMember]
		NotForUpToDate = 0x20,
		[ReleasedEnumMember]
		TimeStampOnly = 0x40,
		[ReleasedEnumMember]
		Compiled = 0x80,
		[ReleasedEnumMember]
		BootProjectRelevant = 0x100,
		[ReleasedEnumMember]
		SavePrecompile = 0x200,
		[ReleasedEnumMember]
		ContainsDirVarAccess = 0x400,
		[ReleasedEnumMember]
		DataRelocations = 0x800,
		[ReleasedEnumMember]
		ContainsNoParseTree = 0x1000,
		[ReleasedEnumMember]
		ContainsNoCode = 0x2000,
		[ReleasedEnumMember]
		ToTypify = 0x4000,
		[ReleasedEnumMember]
		NoParseTreeLoaded = 0x8000,
		[ReleasedEnumMember]
		Generated = 0x10000,
		[ReleasedEnumMember]
		Blob = 0x80000,
		[ReleasedEnumMember]
		ConstBlob = 0x100000,
		[ReleasedEnumMember]
		ToGenerate = 0x200000,
		[ReleasedEnumMember]
		New = 0x400000,
		[ReleasedEnumMember]
		Linked = 0x800000,
		[ReleasedEnumMember]
		ContainsVirtualFunctionCalls = 0x1000000,
		[ReleasedEnumMember]
		IgnoreForChecksum = 0x2000000,
		[ReleasedEnumMember]
		NoCompile = 0x4000000
	}
}
