using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext2 : IPreCompileContext, ICompileContextCommon
	{
		ITaskInfo[] AllTasks { get; }
	}
}
