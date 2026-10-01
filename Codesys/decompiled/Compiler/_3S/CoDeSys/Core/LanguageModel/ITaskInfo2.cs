using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITaskInfo2 : ITaskInfo
	{
		string ParentTaskName { get; }
	}
}
