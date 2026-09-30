using System;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010D RID: 269
	internal class LMDevice : ILMDevice
	{
		// Token: 0x0600144F RID: 5199 RVA: 0x0003C0E7 File Offset: 0x0003B0E7
		internal LMDevice(string stName, IDeviceIdentification devid)
		{
			this.Name = stName;
			this.DeviceIdentification = devid;
		}

		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x06001450 RID: 5200 RVA: 0x0003C0FD File Offset: 0x0003B0FD
		// (set) Token: 0x06001451 RID: 5201 RVA: 0x0003C105 File Offset: 0x0003B105
		public string Name { get; set; }

		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x06001452 RID: 5202 RVA: 0x0003C10E File Offset: 0x0003B10E
		// (set) Token: 0x06001453 RID: 5203 RVA: 0x0003C116 File Offset: 0x0003B116
		public IDeviceIdentification DeviceIdentification { get; set; }
	}
}
