using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{0148E1B6-6651-4ac0-91AD-DA620ACAD399}")]
	public enum CompiledCodeFlags : ushort
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Relocated = 1
	}
}
