using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPOU
	{
		int Slot { get; set; }

		Guid TaskReference { get; set; }

		int DownloadSlot { get; set; }

		int OnlineChangeSlot { get; set; }

		string Name { get; set; }

		ISequenceStatement Interface { get; set; }

		ISequenceStatement Body { get; set; }

		Guid POUGuid { get; set; }

		Guid ParentObjectGuid { get; set; }

		Guid MessageGuid { get; set; }

		bool Action { get; set; }

		bool External { get; set; }

		bool EnableSystemCall { get; set; }

		bool InhibitOnlineChange { get; set; }

		Guid ObjectGuid { get; set; }
	}
}
