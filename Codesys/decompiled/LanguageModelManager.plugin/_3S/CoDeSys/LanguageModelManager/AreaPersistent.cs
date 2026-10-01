using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012A RID: 298
	public class AreaPersistent : IPersistentArea, IArea
	{
		// Token: 0x060019C0 RID: 6592 RVA: 0x0004A28F File Offset: 0x0004928F
		public AreaPersistent(IArea area, uint uiChecksum)
		{
			this._area = area;
			this.Checksum = uiChecksum;
		}

		// Token: 0x170006AA RID: 1706
		// (get) Token: 0x060019C1 RID: 6593 RVA: 0x0004A2A5 File Offset: 0x000492A5
		public uint Checksum { get; }

		// Token: 0x170006AB RID: 1707
		// (get) Token: 0x060019C2 RID: 6594 RVA: 0x0004A2AD File Offset: 0x000492AD
		public int Index
		{
			get
			{
				return this._area.Index;
			}
		}

		// Token: 0x170006AC RID: 1708
		// (get) Token: 0x060019C3 RID: 6595 RVA: 0x0004A2BA File Offset: 0x000492BA
		public int Size
		{
			get
			{
				return this._area.Size;
			}
		}

		// Token: 0x170006AD RID: 1709
		// (get) Token: 0x060019C4 RID: 6596 RVA: 0x0004A2C7 File Offset: 0x000492C7
		public int StartAddress
		{
			get
			{
				return this._area.StartAddress;
			}
		}

		// Token: 0x170006AE RID: 1710
		// (get) Token: 0x060019C5 RID: 6597 RVA: 0x0004A2D4 File Offset: 0x000492D4
		public bool Automatic
		{
			get
			{
				return this._area.Automatic;
			}
		}

		// Token: 0x170006AF RID: 1711
		// (get) Token: 0x060019C6 RID: 6598 RVA: 0x0004A2E1 File Offset: 0x000492E1
		public DataSegmentFlags Flags
		{
			get
			{
				return this._area.Flags;
			}
		}

		// Token: 0x04000535 RID: 1333
		private readonly IArea _area;
	}
}
