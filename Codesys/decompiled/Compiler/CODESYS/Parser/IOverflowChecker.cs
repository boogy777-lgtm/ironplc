using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IOverflowChecker
	{
		bool CheckOverflow(TypeClass tc, bool bSign, ulong nValue);
	}
}
