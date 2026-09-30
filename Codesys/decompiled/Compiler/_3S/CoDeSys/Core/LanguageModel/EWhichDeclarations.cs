using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EWhichDeclarations
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Locals = 1,
		[ReleasedEnumMember]
		SystemLibraries = 2,
		[ReleasedEnumMember]
		Namespaces = 4,
		[ReleasedEnumMember]
		POUsFromSubLibraries = 8
	}
}
