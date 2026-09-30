using System;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010E RID: 270
	internal class LMApplication : ILMApplication4, ILMApplication3, ILMApplication2, ILMApplication
	{
		// Token: 0x06001454 RID: 5204 RVA: 0x0003C11F File Offset: 0x0003B11F
		public LMApplication(Guid parentApp, string stDeviceName, string stApplicationName, IDeviceIdentification devid)
		{
			this.ParentApp = parentApp;
			this.DeviceName = stDeviceName;
			this.ApplicationName = stApplicationName;
			this.DeviceIdentification = devid;
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x06001455 RID: 5205 RVA: 0x0003C15A File Offset: 0x0003B15A
		// (set) Token: 0x06001456 RID: 5206 RVA: 0x0003C162 File Offset: 0x0003B162
		public Guid ParentApp { get; set; }

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x06001457 RID: 5207 RVA: 0x0003C16B File Offset: 0x0003B16B
		// (set) Token: 0x06001458 RID: 5208 RVA: 0x0003C173 File Offset: 0x0003B173
		public Guid MemorySettingsProvider { get; set; } = Guid.Empty;

		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x06001459 RID: 5209 RVA: 0x0003C17C File Offset: 0x0003B17C
		// (set) Token: 0x0600145A RID: 5210 RVA: 0x0003C184 File Offset: 0x0003B184
		public string DeviceName { get; set; }

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x0600145B RID: 5211 RVA: 0x0003C18D File Offset: 0x0003B18D
		// (set) Token: 0x0600145C RID: 5212 RVA: 0x0003C195 File Offset: 0x0003B195
		public string ApplicationName { get; set; }

		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x0600145D RID: 5213 RVA: 0x0003C19E File Offset: 0x0003B19E
		// (set) Token: 0x0600145E RID: 5214 RVA: 0x0003C1A6 File Offset: 0x0003B1A6
		public string SimulationApplicationName { get; set; } = string.Empty;

		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x0600145F RID: 5215 RVA: 0x0003C1AF File Offset: 0x0003B1AF
		// (set) Token: 0x06001460 RID: 5216 RVA: 0x0003C1B7 File Offset: 0x0003B1B7
		public IDeviceIdentification DeviceIdentification { get; set; }

		// Token: 0x17000595 RID: 1429
		// (get) Token: 0x06001461 RID: 5217 RVA: 0x0003C1C0 File Offset: 0x0003B1C0
		// (set) Token: 0x06001462 RID: 5218 RVA: 0x0003C1C8 File Offset: 0x0003B1C8
		public int DynamicMemorySize { get; set; }

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x06001463 RID: 5219 RVA: 0x0003C1D1 File Offset: 0x0003B1D1
		// (set) Token: 0x06001464 RID: 5220 RVA: 0x0003C1D9 File Offset: 0x0003B1D9
		public int TargetOutputSize { get; set; }

		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x06001465 RID: 5221 RVA: 0x0003C1E2 File Offset: 0x0003B1E2
		// (set) Token: 0x06001466 RID: 5222 RVA: 0x0003C1EA File Offset: 0x0003B1EA
		public int TargetInputSize { get; set; }

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x06001467 RID: 5223 RVA: 0x0003C1F3 File Offset: 0x0003B1F3
		// (set) Token: 0x06001468 RID: 5224 RVA: 0x0003C1FB File Offset: 0x0003B1FB
		public int TargetMemorySize { get; set; }

		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x06001469 RID: 5225 RVA: 0x0003C204 File Offset: 0x0003B204
		// (set) Token: 0x0600146A RID: 5226 RVA: 0x0003C20C File Offset: 0x0003B20C
		public int TargetStaticSize { get; set; }

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x0600146B RID: 5227 RVA: 0x0003C215 File Offset: 0x0003B215
		// (set) Token: 0x0600146C RID: 5228 RVA: 0x0003C21D File Offset: 0x0003B21D
		public bool DeviceApplication { get; set; }

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x0600146D RID: 5229 RVA: 0x0003C226 File Offset: 0x0003B226
		// (set) Token: 0x0600146E RID: 5230 RVA: 0x0003C22E File Offset: 0x0003B22E
		public bool GenerateContent { get; set; }
	}
}
