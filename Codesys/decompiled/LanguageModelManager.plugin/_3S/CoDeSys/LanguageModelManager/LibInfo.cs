using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CC RID: 204
	[TypeGuid("{fe2c119a-1ed7-4e57-b634-ac40dd8d0fba}")]
	[StorageVersion("3.3.0.0")]
	public class LibInfo : GenericObject2, _ILibInfo
	{
		// Token: 0x06000E84 RID: 3716 RVA: 0x0000AC39 File Offset: 0x00009C39
		public LibInfo()
		{
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x00026C02 File Offset: 0x00025C02
		public LibInfo(string stPath, int nId, string stNamespace, bool bLibReference, bool bPoolReference)
		{
			this.m_stPath = stPath;
			this.m_nId = nId;
			this.m_stNamespace = stNamespace;
			this.m_bLibraryReference = bLibReference;
			this.m_bPoolReference = bPoolReference;
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06000E86 RID: 3718 RVA: 0x00026C2F File Offset: 0x00025C2F
		public string Path
		{
			get
			{
				return this.m_stPath;
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06000E87 RID: 3719 RVA: 0x00026C37 File Offset: 0x00025C37
		public int Id
		{
			get
			{
				return this.m_nId;
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06000E88 RID: 3720 RVA: 0x00026C3F File Offset: 0x00025C3F
		public string Namespace
		{
			get
			{
				return this.m_stNamespace;
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06000E89 RID: 3721 RVA: 0x00026C47 File Offset: 0x00025C47
		public bool LibReference
		{
			get
			{
				return this.m_bLibraryReference;
			}
		}

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x06000E8A RID: 3722 RVA: 0x00026C4F File Offset: 0x00025C4F
		public bool PoolReference
		{
			get
			{
				return this.m_bPoolReference;
			}
		}

		// Token: 0x0400028E RID: 654
		[DefaultSerialization("Path")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stPath;

		// Token: 0x0400028F RID: 655
		[DefaultSerialization("Id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nId;

		// Token: 0x04000290 RID: 656
		[DefaultSerialization("Namespace")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stNamespace;

		// Token: 0x04000291 RID: 657
		[DefaultSerialization("LibReference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bLibraryReference;

		// Token: 0x04000292 RID: 658
		[DefaultSerialization("PoolReference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bPoolReference;
	}
}
