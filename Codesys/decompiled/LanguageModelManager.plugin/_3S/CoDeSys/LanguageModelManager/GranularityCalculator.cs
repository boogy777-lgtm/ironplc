using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Legacy;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000039 RID: 57
	public static class GranularityCalculator
	{
		// Token: 0x060002BC RID: 700 RVA: 0x0000AA77 File Offset: 0x00009A77
		public static int GetGranularity(ICompiledType type, int iMinSize, IPrecompileScope4 prescope)
		{
			if (VersionedCompilerFactory._GranularityCalculator_OrNull == null)
			{
				return GranularityCalculator.GetGranularity(type, iMinSize, prescope);
			}
			return VersionedCompilerFactory._GranularityCalculator_OrNull.GetGranularity(type, iMinSize, prescope);
		}
	}
}
