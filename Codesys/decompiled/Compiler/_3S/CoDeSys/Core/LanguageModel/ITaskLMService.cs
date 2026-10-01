using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITaskLMService
	{
		string GeneratedTaskConfigStruct { get; }

		string GetTaskMemberName(string stTaskName);
	}
}
