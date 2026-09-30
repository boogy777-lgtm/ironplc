using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext5 : ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		bool IsUpToDate(out bool bOnlineChangePossible);
	}
}
