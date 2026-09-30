using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	[TypeGuid("{289abe0c-adaf-497d-838c-4cd2bf738076}")]
	public enum VarExprFlag : byte
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		NoVirtual = 1,
		[ReleasedEnumMember]
		WriteAccess = 2,
		[ReleasedEnumMember]
		FormalParamsParameter = 4,
		[ReleasedEnumMember]
		InitializingWriteAccess = 8,
		[ReleasedEnumMember]
		PrecompileTypified = 0x10
	}
}
