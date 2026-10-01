using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IApplicationDeviceTable2 : _IApplicationDeviceTable
	{
		IDictionary<Guid, Guid> DeviceOfApplication { get; }

		IDictionary<Guid, ICollection> ApplicationsOfDevice { get; }

		IDictionary<Guid, string> ApplicationNameTable { get; }

		IDictionary<Guid, string> SimulationApplicationNameTable { get; }

		IDictionary<Guid, string> DeviceNameTable { get; }

		IDictionary<Guid, IDeviceIdentification> TargetIdOfDevice { get; }

		IDictionary<Guid, ICollection> ClonesOfApplication { get; }

		IDictionary<Guid, Guid> SubApplicationTable { get; }

		IDictionary<Guid, Guid> MemorySettingsProviderTable { get; }
	}
}
