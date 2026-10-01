using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x0200025C RID: 604
	[TypeGuid("{9a2297c9-1fa0-4ec6-a2b2-e2de8fdc6a63}")]
	[StorageVersion("3.3.0.0")]
	public class RelativeBitLocation : RelativeLocation
	{
		// Token: 0x06002836 RID: 10294 RVA: 0x00064B7A File Offset: 0x00063B7A
		public RelativeBitLocation()
		{
		}

		// Token: 0x06002837 RID: 10295 RVA: 0x00064B82 File Offset: 0x00063B82
		public RelativeBitLocation(int iOffset, byte byBitLocation) : base(iOffset)
		{
			this.m_byBitNr = byBitLocation;
		}

		// Token: 0x17000B36 RID: 2870
		// (get) Token: 0x06002838 RID: 10296 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsBitLocation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B37 RID: 2871
		// (get) Token: 0x06002839 RID: 10297 RVA: 0x00064B92 File Offset: 0x00063B92
		// (set) Token: 0x0600283A RID: 10298 RVA: 0x00064B9A File Offset: 0x00063B9A
		public override byte BitNr
		{
			get
			{
				return this.m_byBitNr;
			}
			set
			{
				this.m_byBitNr = value;
			}
		}

		// Token: 0x040007A3 RID: 1955
		[DefaultSerialization("BitNr")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private byte m_byBitNr;
	}
}
