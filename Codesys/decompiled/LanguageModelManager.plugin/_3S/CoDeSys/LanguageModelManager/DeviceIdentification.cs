using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000138 RID: 312
	[TypeGuid("{d774556f-de05-4539-a455-16fd4345bbf5}")]
	[StorageVersion("3.3.0.0")]
	public class DeviceIdentification : GenericObject2, IDeviceIdentification
	{
		// Token: 0x06001AB3 RID: 6835 RVA: 0x0000AC39 File Offset: 0x00009C39
		public DeviceIdentification()
		{
		}

		// Token: 0x06001AB4 RID: 6836 RVA: 0x0004C29A File Offset: 0x0004B29A
		public DeviceIdentification(int nType, string stId, string stVersion)
		{
			this._nType = nType;
			this._stId = stId;
			this._stVersion = stVersion;
		}

		// Token: 0x17000717 RID: 1815
		// (get) Token: 0x06001AB5 RID: 6837 RVA: 0x0004C2B7 File Offset: 0x0004B2B7
		public int Type
		{
			get
			{
				return this._nType;
			}
		}

		// Token: 0x17000718 RID: 1816
		// (get) Token: 0x06001AB6 RID: 6838 RVA: 0x0004C2BF File Offset: 0x0004B2BF
		public string Id
		{
			get
			{
				return this._stId;
			}
		}

		// Token: 0x17000719 RID: 1817
		// (get) Token: 0x06001AB7 RID: 6839 RVA: 0x0004C2C7 File Offset: 0x0004B2C7
		public string Version
		{
			get
			{
				return this._stVersion;
			}
		}

		// Token: 0x04000590 RID: 1424
		[DefaultSerialization("type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int _nType;

		// Token: 0x04000591 RID: 1425
		[DefaultSerialization("id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stId;

		// Token: 0x04000592 RID: 1426
		[DefaultSerialization("version")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stVersion;
	}
}
