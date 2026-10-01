using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory2
	{
		_IBreakPointStatement CreateBreakPointStatement(long bpPosition, long successorPosition);
	}
}
