using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodePosition2 : ICodePosition
	{
		bool GetAccessFlag(AccessFlag acc);
	}
}
