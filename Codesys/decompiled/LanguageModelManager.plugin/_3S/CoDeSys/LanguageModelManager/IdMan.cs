using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000150 RID: 336
	[TypeGuid("{1bef2131-34c3-45be-922f-9a02d282ff7b}")]
	[StorageVersion("3.3.0.0")]
	public class IdMan : GenericObject2
	{
		// Token: 0x06001B89 RID: 7049 RVA: 0x0004E1DC File Offset: 0x0004D1DC
		public virtual int GetNext()
		{
			int iCurrent = this.m_iCurrent;
			this.m_iCurrent = iCurrent + 1;
			return iCurrent;
		}

		// Token: 0x1700076A RID: 1898
		// (get) Token: 0x06001B8A RID: 7050 RVA: 0x0004E1FA File Offset: 0x0004D1FA
		// (set) Token: 0x06001B8B RID: 7051 RVA: 0x0004E202 File Offset: 0x0004D202
		public virtual int Current
		{
			get
			{
				return this.m_iCurrent;
			}
			set
			{
				this.m_iCurrent = value;
			}
		}

		// Token: 0x040005CE RID: 1486
		[DefaultSerialization("Current")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iCurrent;
	}
}
