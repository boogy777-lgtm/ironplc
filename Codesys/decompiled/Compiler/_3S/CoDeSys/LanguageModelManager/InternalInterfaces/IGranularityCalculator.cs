using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IGranularityCalculator
	{
		int GetGranularity(ICompiledType type, int iMinSize, IPrecompileScope4 prescope);
	}
}
