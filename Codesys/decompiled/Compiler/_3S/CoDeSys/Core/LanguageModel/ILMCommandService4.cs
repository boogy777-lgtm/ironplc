using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCommandService4 : ILMCommandService3, ILMCommandService2, ILMCommandService
	{
		void SimulationModeChanged(Guid guidDevice, bool newSimulationMode);
	}
}
