using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IMemorySettingsHelper
	{
		_IMemorySettings DefaultMemorySettings { get; }

		IMemorySettingsProvider GetMemorySettingsProvider(Guid guidApplication);

		_IMemorySettings GetChildApplicationMemorySettings(Guid guidDevice, Guid guidApplication);

		_IMemorySettings GetMemorySettings(Guid guidApplication, bool bSimulation);

		void UpdateTargetMemorySettings(Guid guidApplication, _IMemorySettings memset);

		_IMemorySettings GetMemorySettings(Guid guidDevice, Guid guidParentApplication, Guid guidApplication, bool bSimulation);
	}
}
