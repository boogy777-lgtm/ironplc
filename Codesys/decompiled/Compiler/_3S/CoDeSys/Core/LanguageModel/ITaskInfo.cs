using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITaskInfo
	{
		Guid ObjectGuid { get; }

		Guid TaskGuid { get; }

		string TaskName { get; }
	}
}
