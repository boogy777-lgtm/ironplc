using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ITaskList
	{
		int Count { get; }

		ITaskInfo this[int n] { get; }

		ITaskInfo this[Guid guidTask] { get; }

		ITaskInfo this[string stName] { get; }

		void Clear();

		byte GetTaskIndexByGuid(Guid guidTask);

		byte GetTaskIndexByName(string stName);

		void AddTaskInfo(Guid guidObject, Guid guidTask, string stName);

		void AddTaskInfo(Guid guidObject, Guid guidTask, string stName, string stParentTaskName);

		void RemoveTaskInfo(Guid guidObject);

		bool IsEqual(_ITaskList tlRef);

		_ITaskList Duplicate();
	}
}
