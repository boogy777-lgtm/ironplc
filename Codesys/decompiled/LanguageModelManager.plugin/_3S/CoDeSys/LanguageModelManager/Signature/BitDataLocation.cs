using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000252 RID: 594
	[TypeGuid("{49cb97d6-e01d-4132-a73c-db4c3a2868fe}")]
	[StorageVersion("3.3.0.0")]
	public class BitDataLocation : DataLocation
	{
		// Token: 0x060027DA RID: 10202 RVA: 0x00063D0D File Offset: 0x00062D0D
		public BitDataLocation()
		{
		}

		// Token: 0x060027DB RID: 10203 RVA: 0x00063D15 File Offset: 0x00062D15
		public BitDataLocation(ushort usArea, int iOffset, byte byBitLocation) : base(usArea, iOffset)
		{
			this.m_byBitNr = byBitLocation;
		}

		// Token: 0x17000B17 RID: 2839
		// (get) Token: 0x060027DC RID: 10204 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsBitLocation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B18 RID: 2840
		// (get) Token: 0x060027DD RID: 10205 RVA: 0x00063D26 File Offset: 0x00062D26
		// (set) Token: 0x060027DE RID: 10206 RVA: 0x00063D2E File Offset: 0x00062D2E
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

		// Token: 0x04000780 RID: 1920
		[DefaultSerialization("BitNr")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private byte m_byBitNr;
	}
}
