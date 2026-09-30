using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope5 : IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope
	{
		IIdentifierInfo[] GetIdentifierInfo(string stAccessPath);
	}
}
