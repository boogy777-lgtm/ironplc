using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILMEntity
	{
		string Name { get; set; }

		ISequenceStatement Interface { get; set; }

		Guid ObjectGuid { get; set; }

		bool InhibitOnlineChange { get; set; }

		Guid LanguageModelOfObject { get; set; }

		string CompilerDefines { get; set; }

		SignatureFlag DefaultFlag { get; set; }

		Guid ParentObjectGuid { get; set; }
	}
}
