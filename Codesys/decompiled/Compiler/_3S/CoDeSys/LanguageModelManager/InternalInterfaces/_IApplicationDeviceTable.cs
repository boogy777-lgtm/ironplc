using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IApplicationDeviceTable
	{
		void Clear();

		void SetDeviceName(Guid guidDevice, string stName);

		string GetDeviceName(Guid guidDevice);

		void SetApplicationName(Guid guidApplication, string stName, bool bSimulation);

		string GetApplicationName(Guid guidApplication, bool bSimulation);

		Guid GetDeviceGuidByName(string stName);

		Guid GetApplicationGuidByName(string stName);

		Guid GetParentApplication(Guid guidSubApplication);

		void SetParentApplication(Guid guidParentApplication, Guid guidSubApplication);

		IEnumerable<Guid> GetChildApplications(Guid guidParentApplication, bool bRecursive);

		string GetApplicationNameByGuid(Guid guidApplication, bool bSimulation);

		void AddApplicationDevice(Guid guidApplication, Guid guidDevice);

		Guid[] GetApplicationsOfDevice(Guid guidDevice);

		Guid GetDeviceOfApplication(Guid guidApplication);

		IDeviceIdentification GetTargetIdOfDevice(Guid guidDevice);

		void SetTargetIdOfDevice(Guid guidDevice, IDeviceIdentification devId);

		void AddCloneOfApplication(Guid guidApplication, Guid guidClone);

		Guid[] GetClonesOfApplication(Guid guidApplication);

		Guid GetOriginalApplication(Guid guidApplication);

		void RemoveByGuid(Guid guid);

		Guid GetMemorySettingsProvider(Guid guidApplication);

		void SetMemorySettingsProvider(Guid guidApplication, Guid guidMemorySettingsProvider);
	}
}
