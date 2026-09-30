using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000128 RID: 296
	[TypeGuid("{1cdc5830-00b6-4d16-ac84-61a1c00364e9}")]
	[StorageVersion("3.3.0.0")]
	public class MemManGap : GenericObject2, _IMemManGap, IMemManGap
	{
		// Token: 0x0600199E RID: 6558 RVA: 0x0000AC39 File Offset: 0x00009C39
		public MemManGap()
		{
		}

		// Token: 0x0600199F RID: 6559 RVA: 0x0004965D File Offset: 0x0004865D
		public MemManGap(int iOffset, int iSize)
		{
			this.m_iOffset = iOffset;
			this.m_iSize = iSize;
		}

		// Token: 0x170006A1 RID: 1697
		// (get) Token: 0x060019A0 RID: 6560 RVA: 0x00049673 File Offset: 0x00048673
		// (set) Token: 0x060019A1 RID: 6561 RVA: 0x0004967B File Offset: 0x0004867B
		public int Offset
		{
			get
			{
				return this.m_iOffset;
			}
			set
			{
				this.m_iOffset = value;
			}
		}

		// Token: 0x170006A2 RID: 1698
		// (get) Token: 0x060019A2 RID: 6562 RVA: 0x00049684 File Offset: 0x00048684
		// (set) Token: 0x060019A3 RID: 6563 RVA: 0x0004968C File Offset: 0x0004868C
		public int Size
		{
			get
			{
				return this.m_iSize;
			}
			set
			{
				this.m_iSize = value;
			}
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x00049695 File Offset: 0x00048695
		public _IMemManGap Duplicate()
		{
			return new MemManGap(this.m_iOffset, this.m_iSize);
		}

		// Token: 0x04000530 RID: 1328
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Offset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iOffset;

		// Token: 0x04000531 RID: 1329
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iSize;
	}
}
