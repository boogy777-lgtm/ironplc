using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCallTreeService
	{
		IStackUsage GetMaxStackUsage(Guid gdApplication, bool bCalculatePositions, string stTaskName);

		IStackUsage GetMaxStackUsage(Guid gdApplication, bool bCalculatePositions, string stTaskName, uint uiMaxStackSize);
	}
}
