using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000255 RID: 597
	[TypeGuid("{48397926-5c6f-41f7-b878-4ffb8fbbd3f0}")]
	[StorageVersion("3.3.0.0")]
	public class DataLocation : GenericObject2, _IDataLocation, IDataLocation2, IDataLocation
	{
		// Token: 0x060027FE RID: 10238 RVA: 0x0000AC39 File Offset: 0x00009C39
		public DataLocation()
		{
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x00064618 File Offset: 0x00063618
		public DataLocation(ushort usArea, int iOffset)
		{
			this.m_usArea = usArea;
			this.m_iOffset = iOffset;
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x00064630 File Offset: 0x00063630
		public virtual bool IsEqual(IDataLocation locIn)
		{
			return !(locIn.GetType() != base.GetType()) && locIn.Area == this.Area && locIn.BitNr == this.BitNr && locIn.Offset == this.Offset;
		}

		// Token: 0x06002801 RID: 10241 RVA: 0x00064680 File Offset: 0x00063680
		public override int CompareTo(object obj)
		{
			IDataLocation dataLocation = obj as IDataLocation;
			if (dataLocation == null)
			{
				return 0;
			}
			if (((IDataLocation)this).Area < dataLocation.Area)
			{
				return -1;
			}
			if (((IDataLocation)this).Area > dataLocation.Area)
			{
				return 1;
			}
			if (((IDataLocation)this).Offset < dataLocation.Offset)
			{
				return -1;
			}
			if (((IDataLocation)this).Offset > dataLocation.Offset)
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x17000B21 RID: 2849
		// (get) Token: 0x06002802 RID: 10242 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsRelativ
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B22 RID: 2850
		// (get) Token: 0x06002803 RID: 10243 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsBitLocation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B23 RID: 2851
		// (get) Token: 0x06002804 RID: 10244 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasRetainLocation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B24 RID: 2852
		// (get) Token: 0x06002805 RID: 10245 RVA: 0x000646DC File Offset: 0x000636DC
		// (set) Token: 0x06002806 RID: 10246 RVA: 0x000646E4 File Offset: 0x000636E4
		public virtual ushort Area
		{
			get
			{
				return this.m_usArea;
			}
			set
			{
				this.m_usArea = value;
			}
		}

		// Token: 0x17000B25 RID: 2853
		// (get) Token: 0x06002807 RID: 10247 RVA: 0x000646ED File Offset: 0x000636ED
		// (set) Token: 0x06002808 RID: 10248 RVA: 0x000646F5 File Offset: 0x000636F5
		public virtual int Offset
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

		// Token: 0x17000B26 RID: 2854
		// (get) Token: 0x06002809 RID: 10249 RVA: 0x000646FE File Offset: 0x000636FE
		// (set) Token: 0x0600280A RID: 10250 RVA: 0x00064705 File Offset: 0x00063705
		public virtual byte BitNr
		{
			get
			{
				return byte.MaxValue;
			}
			set
			{
				throw new NotSupportedException("Use Specialized data location for bits ");
			}
		}

		// Token: 0x17000B27 RID: 2855
		// (get) Token: 0x0600280B RID: 10251 RVA: 0x0004B36C File Offset: 0x0004A36C
		// (set) Token: 0x0600280C RID: 10252 RVA: 0x00064711 File Offset: 0x00063711
		public virtual ushort AreaRetain
		{
			get
			{
				return ushort.MaxValue;
			}
			set
			{
				throw new NotSupportedException("No longer used");
			}
		}

		// Token: 0x17000B28 RID: 2856
		// (get) Token: 0x0600280D RID: 10253 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x0600280E RID: 10254 RVA: 0x00064711 File Offset: 0x00063711
		public virtual int OffsetRetain
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
				throw new NotSupportedException("No longer used");
			}
		}

		// Token: 0x0600280F RID: 10255 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool GetFlag(DataLocationFlag dlFlag)
		{
			return false;
		}

		// Token: 0x0400078D RID: 1933
		[DefaultSerialization("Area")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ushort m_usArea;

		// Token: 0x0400078E RID: 1934
		[DefaultSerialization("Offset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iOffset;
	}
}
