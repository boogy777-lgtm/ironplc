using System;
using System.Reflection;
using System.Xml;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012B RID: 299
	[TypeGuid("{e22c2694-f644-4661-a028-99897a56f19a}")]
	[StorageVersion("3.3.0.0")]
	public class Area : GenericObject2, _IArea, IArea4, IArea3, IArea2, IArea, IGenericInterfaceExtensionProvider
	{
		// Token: 0x060019C7 RID: 6599 RVA: 0x0004A2EE File Offset: 0x000492EE
		public Area()
		{
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x0004A314 File Offset: 0x00049314
		public Area(DataSegmentFlags datasegmentflags, AreaFlags areaflags)
		{
			this.m_iStartAddress = 0;
			this.m_iSize = int.MaxValue;
			this.m_nIndex = 0;
			this.m_datasegmentflags = datasegmentflags;
			this.SetAreaFlag(AreaFlags.Automatic | AreaFlags.DynamicSize | areaflags, true);
			this.m_iMinAreaSize = 0;
			this.m_iAllocationPlusInPercent = 0;
		}

		// Token: 0x170006B0 RID: 1712
		// (get) Token: 0x060019C9 RID: 6601 RVA: 0x0004A37C File Offset: 0x0004937C
		// (set) Token: 0x060019CA RID: 6602 RVA: 0x0004A385 File Offset: 0x00049385
		public bool Dynamic
		{
			get
			{
				return this.GetAreaFlag(AreaFlags.DynamicSize);
			}
			set
			{
				this.SetAreaFlag(AreaFlags.DynamicSize, value);
			}
		}

		// Token: 0x170006B1 RID: 1713
		// (get) Token: 0x060019CB RID: 6603 RVA: 0x0004A38F File Offset: 0x0004938F
		// (set) Token: 0x060019CC RID: 6604 RVA: 0x0004A397 File Offset: 0x00049397
		public uint Checksum
		{
			get
			{
				return this._uiChecksum;
			}
			set
			{
				this._uiChecksum = value;
			}
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x0004A3A0 File Offset: 0x000493A0
		public bool GetAreaFlag(AreaFlags aflag)
		{
			return (this.m_areaflags & aflag) == aflag;
		}

		// Token: 0x060019CE RID: 6606 RVA: 0x0004A3AD File Offset: 0x000493AD
		public void SetAreaFlag(AreaFlags aflag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_areaflags |= aflag;
				return;
			}
			this.m_areaflags &= ~aflag;
		}

		// Token: 0x170006B2 RID: 1714
		// (get) Token: 0x060019CF RID: 6607 RVA: 0x0004A3D0 File Offset: 0x000493D0
		public AreaFlags AreaFlags
		{
			get
			{
				return this.m_areaflags;
			}
		}

		// Token: 0x060019D0 RID: 6608 RVA: 0x0004A3D8 File Offset: 0x000493D8
		public bool GetDataSegmentFlag(DataSegmentFlags dsFlag)
		{
			return (this.m_datasegmentflags & dsFlag) == dsFlag;
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x0004A3E5 File Offset: 0x000493E5
		public void SetDataSegmentFlag(DataSegmentFlags dsFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_datasegmentflags |= dsFlag;
				return;
			}
			this.m_datasegmentflags &= ~dsFlag;
		}

		// Token: 0x170006B3 RID: 1715
		// (get) Token: 0x060019D2 RID: 6610 RVA: 0x0004A409 File Offset: 0x00049409
		public DataSegmentFlags DataSegmentFlags
		{
			get
			{
				return this.m_datasegmentflags;
			}
		}

		// Token: 0x060019D3 RID: 6611 RVA: 0x0004A414 File Offset: 0x00049414
		public _IArea Duplicate()
		{
			Area area = new Area(this.m_datasegmentflags, this.m_areaflags);
			area.SetAreaFlag(this.m_areaflags, true);
			area.m_iStartAddress = this.m_iStartAddress;
			area.m_iSize = this.m_iSize;
			area.m_nIndex = this.m_nIndex;
			area.m_iMinAreaSize = this.m_iMinAreaSize;
			area.m_iMaxAreaSize = this.m_iMaxAreaSize;
			area.m_areaflags = this.m_areaflags;
			area.m_datasegmentflags = this.m_datasegmentflags;
			area.m_iAllocationPlusInPercent = this.m_iAllocationPlusInPercent;
			area.m_iAvailableSize = this.m_iAvailableSize;
			return area;
		}

		// Token: 0x170006B4 RID: 1716
		// (get) Token: 0x060019D4 RID: 6612 RVA: 0x0004A4AB File Offset: 0x000494AB
		// (set) Token: 0x060019D5 RID: 6613 RVA: 0x0004A4B3 File Offset: 0x000494B3
		public int MinimalAreaSize
		{
			get
			{
				return this.m_iMinAreaSize;
			}
			set
			{
				this.m_iMinAreaSize = value;
			}
		}

		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x060019D6 RID: 6614 RVA: 0x0004A4BC File Offset: 0x000494BC
		// (set) Token: 0x060019D7 RID: 6615 RVA: 0x0004A4C4 File Offset: 0x000494C4
		public int MaximalAreaSize
		{
			get
			{
				return this.m_iMaxAreaSize;
			}
			set
			{
				this.m_iMaxAreaSize = value;
			}
		}

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x060019D8 RID: 6616 RVA: 0x0004A4CD File Offset: 0x000494CD
		// (set) Token: 0x060019D9 RID: 6617 RVA: 0x0004A4D5 File Offset: 0x000494D5
		public int AllocationPlusInPercent
		{
			get
			{
				return this.m_iAllocationPlusInPercent;
			}
			set
			{
				this.m_iAllocationPlusInPercent = value;
			}
		}

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x060019DA RID: 6618 RVA: 0x0004A4DE File Offset: 0x000494DE
		// (set) Token: 0x060019DB RID: 6619 RVA: 0x0004A4E6 File Offset: 0x000494E6
		public int Index
		{
			get
			{
				return this.m_nIndex;
			}
			set
			{
				this.m_nIndex = value;
			}
		}

		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x060019DC RID: 6620 RVA: 0x0004A4EF File Offset: 0x000494EF
		// (set) Token: 0x060019DD RID: 6621 RVA: 0x0004A4F7 File Offset: 0x000494F7
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

		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x060019DE RID: 6622 RVA: 0x0004A500 File Offset: 0x00049500
		// (set) Token: 0x060019DF RID: 6623 RVA: 0x0004A508 File Offset: 0x00049508
		public int AvailableSize
		{
			get
			{
				return this.m_iAvailableSize;
			}
			set
			{
				this.m_iAvailableSize = value;
			}
		}

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x060019E0 RID: 6624 RVA: 0x0004A511 File Offset: 0x00049511
		// (set) Token: 0x060019E1 RID: 6625 RVA: 0x0004A525 File Offset: 0x00049525
		public int StartAddress
		{
			get
			{
				if (this.GetAreaFlag(AreaFlags.Fixed))
				{
					return this.m_iStartAddress;
				}
				return 0;
			}
			set
			{
				this.m_iStartAddress = value;
			}
		}

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x060019E2 RID: 6626 RVA: 0x0004A52E File Offset: 0x0004952E
		// (set) Token: 0x060019E3 RID: 6627 RVA: 0x0004A537 File Offset: 0x00049537
		public bool Automatic
		{
			get
			{
				return this.GetAreaFlag(AreaFlags.Automatic);
			}
			set
			{
				this.SetAreaFlag(AreaFlags.Automatic, value);
			}
		}

		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x060019E4 RID: 6628 RVA: 0x0004A409 File Offset: 0x00049409
		public DataSegmentFlags Flags
		{
			get
			{
				return this.m_datasegmentflags;
			}
		}

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x060019E5 RID: 6629 RVA: 0x0004A541 File Offset: 0x00049541
		public bool OnlineChangeArea
		{
			get
			{
				return this.GetAreaFlag(AreaFlags.OnlineChange);
			}
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x0004A54A File Offset: 0x0004954A
		public void RaiseEvent(string stEvent, XmlDocument eventData)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x060019E7 RID: 6631 RVA: 0x0004A54A File Offset: 0x0004954A
		public void AttachToEvent(string stEvent, GenericEventDelegate callback)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x0004A54A File Offset: 0x0004954A
		public void DetachFromEvent(string stEvent, GenericEventDelegate callback)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x060019E9 RID: 6633 RVA: 0x0004A556 File Offset: 0x00049556
		public bool IsFunctionAvailable(string stFunction)
		{
			return stFunction == "OnlineChangeArea";
		}

		// Token: 0x060019EA RID: 6634 RVA: 0x0004A568 File Offset: 0x00049568
		public XmlDocument CallFunction(string stFunction, XmlDocument functionData)
		{
			if (stFunction == "OnlineChangeArea")
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.AppendChild(xmlDocument.CreateElement("Output"));
				xmlDocument.DocumentElement.InnerText = XmlConvert.ToString(this.OnlineChangeArea);
				return xmlDocument;
			}
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x04000537 RID: 1335
		[DefaultSerialization("StartAddress")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iStartAddress;

		// Token: 0x04000538 RID: 1336
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iSize = int.MaxValue;

		// Token: 0x04000539 RID: 1337
		[DefaultSerialization("Index")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nIndex;

		// Token: 0x0400053A RID: 1338
		[DefaultSerialization("DataSegmentFlags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DataSegmentFlags m_datasegmentflags;

		// Token: 0x0400053B RID: 1339
		[DefaultSerialization("AreaFlags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private AreaFlags m_areaflags;

		// Token: 0x0400053C RID: 1340
		[DefaultSerialization("MinimalSize")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iMinAreaSize;

		// Token: 0x0400053D RID: 1341
		[DefaultSerialization("AllocationPlus")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iAllocationPlusInPercent;

		// Token: 0x0400053E RID: 1342
		[DefaultSerialization("Checksum")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint _uiChecksum;

		// Token: 0x0400053F RID: 1343
		[DefaultSerialization("Available")]
		[StorageVersion("3.5.5.20")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private int m_iAvailableSize = -1;

		// Token: 0x04000540 RID: 1344
		[DefaultSerialization("MaximalSize")]
		[StorageVersion("3.5.6.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private int m_iMaxAreaSize = int.MaxValue;

		// Token: 0x020002B8 RID: 696
		private static class GenericMethods
		{
			// Token: 0x040008C6 RID: 2246
			public const string OnlineChangeArea = "OnlineChangeArea";
		}
	}
}
