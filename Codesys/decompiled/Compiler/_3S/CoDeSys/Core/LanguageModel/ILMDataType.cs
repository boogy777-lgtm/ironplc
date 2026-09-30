using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMDataType
	{
		string Name { get; set; }

		ISequenceStatement Interface { get; set; }

		Guid DUTGuid { get; set; }

		bool InhibitOnlineChange { get; set; }

		Guid ObjectGuid { get; set; }
	}
}
