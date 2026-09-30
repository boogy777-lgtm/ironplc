using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x0200025D RID: 605
	[TypeGuid("{c529d0d0-8cec-4d28-bb44-5e2d4ebecdcf}")]
	[StorageVersion("3.3.0.0")]
	public class RelativeLocation : GenericObject2, _IDataLocation, IDataLocation2, IDataLocation, _IRelativeDataLocation
	{
		// Token: 0x0600283B RID: 10299 RVA: 0x0000AC39 File Offset: 0x00009C39
		public RelativeLocation()
		{
		}

		// Token: 0x0600283C RID: 10300 RVA: 0x00064BA3 File Offset: 0x00063BA3
		public RelativeLocation(int iOffset)
		{
			this.m_iOffset = iOffset;
		}

		// Token: 0x0600283D RID: 10301 RVA: 0x00064BB2 File Offset: 0x00063BB2
		public RelativeLocation(int iOffset, DataLocationFlag dlFlags)
		{
			this.m_iOffset = iOffset;
			this.m_dlFlags = dlFlags;
		}

		// Token: 0x17000B38 RID: 2872
		// (get) Token: 0x0600283E RID: 10302 RVA: 0x00005E58 File Offset: 0x00004E58
		public virtual bool IsRelativ
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B39 RID: 2873
		// (get) Token: 0x0600283F RID: 10303 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsBitLocation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B3A RID: 2874
		// (get) Token: 0x06002840 RID: 10304 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasRetainLocation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B3B RID: 2875
		// (get) Token: 0x06002841 RID: 10305 RVA: 0x0004B36C File Offset: 0x0004A36C
		// (set) Token: 0x06002842 RID: 10306 RVA: 0x00064BC8 File Offset: 0x00063BC8
		public virtual ushort Area
		{
			get
			{
				return ushort.MaxValue;
			}
			set
			{
				throw new NotSupportedException("relative location does not have an area");
			}
		}

		// Token: 0x17000B3C RID: 2876
		// (get) Token: 0x06002843 RID: 10307 RVA: 0x00064BD4 File Offset: 0x00063BD4
		// (set) Token: 0x06002844 RID: 10308 RVA: 0x00064BDC File Offset: 0x00063BDC
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

		// Token: 0x17000B3D RID: 2877
		// (get) Token: 0x06002845 RID: 10309 RVA: 0x000646FE File Offset: 0x000636FE
		// (set) Token: 0x06002846 RID: 10310 RVA: 0x00064BE5 File Offset: 0x00063BE5
		public virtual byte BitNr
		{
			get
			{
				return byte.MaxValue;
			}
			set
			{
				throw new NotSupportedException("relative location does not have a bit nr (there is a specialized class)");
			}
		}

		// Token: 0x17000B3E RID: 2878
		// (get) Token: 0x06002847 RID: 10311 RVA: 0x0004B36C File Offset: 0x0004A36C
		// (set) Token: 0x06002848 RID: 10312 RVA: 0x00064BF1 File Offset: 0x00063BF1
		public virtual ushort AreaRetain
		{
			get
			{
				return ushort.MaxValue;
			}
			set
			{
				throw new NotSupportedException("Not supported anymore");
			}
		}

		// Token: 0x17000B3F RID: 2879
		// (get) Token: 0x06002849 RID: 10313 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x0600284A RID: 10314 RVA: 0x00064BF1 File Offset: 0x00063BF1
		public virtual int OffsetRetain
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
				throw new NotSupportedException("Not supported anymore");
			}
		}

		// Token: 0x0600284B RID: 10315 RVA: 0x00064C00 File Offset: 0x00063C00
		public virtual bool IsEqual(IDataLocation locIn)
		{
			return !(locIn.GetType() != base.GetType()) && locIn.Area == this.Area && locIn.BitNr == this.BitNr && locIn.Offset == this.Offset;
		}

		// Token: 0x0600284C RID: 10316 RVA: 0x00064C4F File Offset: 0x00063C4F
		public bool GetFlag(DataLocationFlag dlFlag)
		{
			return (this.m_dlFlags & dlFlag) == dlFlag;
		}

		// Token: 0x17000B40 RID: 2880
		// (get) Token: 0x0600284D RID: 10317 RVA: 0x00064C5C File Offset: 0x00063C5C
		// (set) Token: 0x0600284E RID: 10318 RVA: 0x00064C64 File Offset: 0x00063C64
		public DataLocationFlag Flags
		{
			get
			{
				return this.m_dlFlags;
			}
			set
			{
				this.m_dlFlags = value;
			}
		}

		// Token: 0x040007A4 RID: 1956
		[DefaultSerialization("Offset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iOffset;

		// Token: 0x040007A5 RID: 1957
		[DefaultSerialization("Flags")]
		[StorageVersion("3.5.5.0")]
		[StorageDefaultValue(DataLocationFlag.None)]
		[Obfuscation(Feature = "rename")]
		private DataLocationFlag m_dlFlags;
	}
}
