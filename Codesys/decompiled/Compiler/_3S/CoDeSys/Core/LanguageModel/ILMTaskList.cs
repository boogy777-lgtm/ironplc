using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMTaskList
	{
		Guid TaskConfigGuid { get; set; }

		ITaskInfo[] Tasks { get; }

		Guid ObjectGuid { get; set; }

		void AddTask(ITaskInfo task);
	}
}
