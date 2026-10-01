using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompiledPOU2 : _ICompiledPOU, ICompiledPOU10, ICompiledPOU9, ICompiledPOU8, ICompiledPOU6, ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU, ICompiledPOU7
	{
		string OriginalName { get; }

		Guid ParentObjectGuid { get; set; }

		CompiledPOUFlags Flags { get; set; }

		InternalCompiledPOUFlags InternalFlags { get; set; }
	}
}
