using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IBreakPointStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IBreakPointStatement
	{
		new long BPPosition { get; set; }

		new long SuccessorPosition { get; set; }
	}
}
