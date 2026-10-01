using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x0200025B RID: 603
	[TypeGuid("{b62cdefb-9e5c-4235-b253-de55073259b1}")]
	[StorageVersion("3.3.0.0")]
	public class ProcessImageLocation : GenericObject2, _IProcessImageLocation, IProcessImageLocation
	{
		// Token: 0x0600282D RID: 10285 RVA: 0x0000AC39 File Offset: 0x00009C39
		public ProcessImageLocation()
		{
		}

		// Token: 0x0600282E RID: 10286 RVA: 0x00064A5C File Offset: 0x00063A5C
		public ProcessImageLocation(IDataLocation datloc, int nSize, DirectVariableLocation dirvarlocation)
		{
			this.m_datloc = datloc;
			this.m_size = nSize;
			this.m_loc = dirvarlocation;
		}

		// Token: 0x0600282F RID: 10287 RVA: 0x00064A79 File Offset: 0x00063A79
		public void AddAccess(AccessFlag access)
		{
			this.m_access |= access;
		}

		// Token: 0x17000B32 RID: 2866
		// (get) Token: 0x06002830 RID: 10288 RVA: 0x00064A89 File Offset: 0x00063A89
		public IDataLocation DataLocation
		{
			get
			{
				return this.m_datloc;
			}
		}

		// Token: 0x17000B33 RID: 2867
		// (get) Token: 0x06002831 RID: 10289 RVA: 0x00064A91 File Offset: 0x00063A91
		public int Size
		{
			get
			{
				return this.m_size;
			}
		}

		// Token: 0x17000B34 RID: 2868
		// (get) Token: 0x06002832 RID: 10290 RVA: 0x00064A99 File Offset: 0x00063A99
		public AccessFlag Access
		{
			get
			{
				return this.m_access;
			}
		}

		// Token: 0x17000B35 RID: 2869
		// (get) Token: 0x06002833 RID: 10291 RVA: 0x00064AA1 File Offset: 0x00063AA1
		public DirectVariableLocation AddressLocation
		{
			get
			{
				return this.m_loc;
			}
		}

		// Token: 0x06002834 RID: 10292 RVA: 0x00064AAC File Offset: 0x00063AAC
		public override int GetHashCode()
		{
			return this.DataLocation.Area.GetHashCode() ^ this.DataLocation.BitNr.GetHashCode() ^ this.DataLocation.Offset.GetHashCode() ^ this.Size.GetHashCode();
		}

		// Token: 0x06002835 RID: 10293 RVA: 0x00064B04 File Offset: 0x00063B04
		public override bool Equals(object obj)
		{
			if (obj is IProcessImageLocation)
			{
				IProcessImageLocation processImageLocation = obj as IProcessImageLocation;
				return this.DataLocation.Area == processImageLocation.DataLocation.Area && this.DataLocation.Offset == processImageLocation.DataLocation.Offset && this.DataLocation.BitNr == processImageLocation.DataLocation.BitNr && this.Size == processImageLocation.Size;
			}
			return false;
		}

		// Token: 0x0400079F RID: 1951
		[DefaultSerialization("DatLoc")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private readonly IDataLocation m_datloc;

		// Token: 0x040007A0 RID: 1952
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private readonly int m_size;

		// Token: 0x040007A1 RID: 1953
		[DefaultSerialization("Access")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private AccessFlag m_access;

		// Token: 0x040007A2 RID: 1954
		[DefaultSerialization("Location")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private readonly DirectVariableLocation m_loc;
	}
}
