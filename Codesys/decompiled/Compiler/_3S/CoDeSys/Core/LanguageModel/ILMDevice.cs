using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMDevice
	{
		string Name { get; set; }

		IDeviceIdentification DeviceIdentification { get; set; }
	}
}
