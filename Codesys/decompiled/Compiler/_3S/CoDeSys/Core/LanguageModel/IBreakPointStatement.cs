using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBreakPointStatement : IStatement, IExprement
	{
		long BPPosition { get; }

		long SuccessorPosition { get; }
	}
}
