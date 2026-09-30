using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014B RID: 331
	[TypeGuid("{032438a5-cfcf-4f2c-b362-37e511ec8f0a}")]
	[StorageVersion("3.3.0.0")]
	public class AddressCrossReference : GenericObject2, IAddressCrossReference, IAddressCrossReferenceSerializable
	{
		// Token: 0x1700075E RID: 1886
		// (get) Token: 0x06001B68 RID: 7016 RVA: 0x0004DC50 File Offset: 0x0004CC50
		// (set) Token: 0x06001B69 RID: 7017 RVA: 0x0004DC78 File Offset: 0x0004CC78
		[DefaultSerialization("PositionsToSave")]
		[StorageVersion("3.3.0.0")]
		private AddressCodePosition[] PositionsToSave
		{
			get
			{
				AddressCodePosition[] array = new AddressCodePosition[this.NumPositions];
				LList<IAddressCodePosition> alPositions = this.m_alPositions;
				IAddressCodePosition[] array2 = array;
				alPositions.CopyTo(array2);
				return array;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.m_alPositions = new LList<IAddressCodePosition>(value.Length);
				this.m_alPositions.AddRange(value);
			}
		}

		// Token: 0x06001B6A RID: 7018 RVA: 0x0000AC39 File Offset: 0x00009C39
		public AddressCrossReference()
		{
		}

		// Token: 0x06001B6B RID: 7019 RVA: 0x0004DC98 File Offset: 0x0004CC98
		public AddressCrossReference(int nCodeId)
		{
			this.m_nCodeId = nCodeId;
			this.m_alPositions = new LList<IAddressCodePosition>();
		}

		// Token: 0x06001B6C RID: 7020 RVA: 0x0004DCB2 File Offset: 0x0004CCB2
		public AddressCrossReference(IAddressCodePosition cp, int nCodeId)
		{
			this.m_nCodeId = nCodeId;
			this.m_alPositions = new LList<IAddressCodePosition>();
			this.m_alPositions.Add(cp);
		}

		// Token: 0x06001B6D RID: 7021 RVA: 0x0004DCD8 File Offset: 0x0004CCD8
		public void AddPosition(IAddressCodePosition cp)
		{
			this.m_alPositions.Add(cp);
		}

		// Token: 0x1700075F RID: 1887
		public IAddressCodePosition this[int n]
		{
			get
			{
				return this.m_alPositions[n];
			}
		}

		// Token: 0x17000760 RID: 1888
		// (get) Token: 0x06001B6F RID: 7023 RVA: 0x0004DCF4 File Offset: 0x0004CCF4
		public int NumPositions
		{
			get
			{
				return this.m_alPositions.Count;
			}
		}

		// Token: 0x17000761 RID: 1889
		// (get) Token: 0x06001B70 RID: 7024 RVA: 0x0004DD01 File Offset: 0x0004CD01
		public int CodeId
		{
			get
			{
				return this.m_nCodeId;
			}
		}

		// Token: 0x17000762 RID: 1890
		// (get) Token: 0x06001B71 RID: 7025 RVA: 0x0004DD0C File Offset: 0x0004CD0C
		public IAddressCodePosition[] Positions
		{
			get
			{
				IAddressCodePosition[] array = new IAddressCodePosition[this.NumPositions];
				this.m_alPositions.CopyTo(array);
				return array;
			}
		}

		// Token: 0x040005C3 RID: 1475
		[Obfuscation(Feature = "rename")]
		private LList<IAddressCodePosition> m_alPositions;

		// Token: 0x040005C4 RID: 1476
		[DefaultSerialization("CodeId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nCodeId;
	}
}
