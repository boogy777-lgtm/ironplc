using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMApplication
	{
		Guid ParentApp { get; set; }

		Guid MemorySettingsProvider { get; set; }

		string DeviceName { get; set; }

		string ApplicationName { get; set; }

		string SimulationApplicationName { get; set; }

		IDeviceIdentification DeviceIdentification { get; set; }

		int DynamicMemorySize { get; set; }
	}
}
