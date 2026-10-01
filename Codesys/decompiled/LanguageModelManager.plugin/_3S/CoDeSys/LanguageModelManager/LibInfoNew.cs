using System;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CD RID: 205
	[TypeGuid("{DF911092-020B-4E0A-BF63-B9DF8A0EE0A7}")]
	[StorageVersion("3.5.3.0")]
	internal class LibInfoNew : GenericObject2, ILibInfoSerializable
	{
		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06000E8D RID: 3725 RVA: 0x00026C60 File Offset: 0x00025C60
		// (set) Token: 0x06000E8C RID: 3724 RVA: 0x00026C57 File Offset: 0x00025C57
		public string LibraryId
		{
			get
			{
				return this.strLibraryId;
			}
			set
			{
				this.strLibraryId = value;
			}
		}

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06000E8F RID: 3727 RVA: 0x00026C71 File Offset: 0x00025C71
		// (set) Token: 0x06000E8E RID: 3726 RVA: 0x00026C68 File Offset: 0x00025C68
		public string Namespace
		{
			get
			{
				return this.strNamespace;
			}
			set
			{
				this.strNamespace = value;
			}
		}

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000E91 RID: 3729 RVA: 0x00026C82 File Offset: 0x00025C82
		// (set) Token: 0x06000E90 RID: 3728 RVA: 0x00026C79 File Offset: 0x00025C79
		public string ReferencingLibrary
		{
			get
			{
				return this.strReferencingLibrary;
			}
			set
			{
				this.strReferencingLibrary = value;
			}
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000E93 RID: 3731 RVA: 0x00026C93 File Offset: 0x00025C93
		// (set) Token: 0x06000E92 RID: 3730 RVA: 0x00026C8A File Offset: 0x00025C8A
		public bool OutOfPool
		{
			get
			{
				return this.bOutOfPool;
			}
			set
			{
				this.bOutOfPool = value;
			}
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000E95 RID: 3733 RVA: 0x00026CA4 File Offset: 0x00025CA4
		// (set) Token: 0x06000E94 RID: 3732 RVA: 0x00026C9B File Offset: 0x00025C9B
		public int Id
		{
			get
			{
				return this.nId;
			}
			set
			{
				this.nId = value;
			}
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000E97 RID: 3735 RVA: 0x00026CB5 File Offset: 0x00025CB5
		// (set) Token: 0x06000E96 RID: 3734 RVA: 0x00026CAC File Offset: 0x00025CAC
		public bool QualifiedOnly
		{
			get
			{
				return this.qualifiedOnly;
			}
			set
			{
				this.qualifiedOnly = value;
			}
		}

		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06000E99 RID: 3737 RVA: 0x00026CC6 File Offset: 0x00025CC6
		// (set) Token: 0x06000E98 RID: 3736 RVA: 0x00026CBD File Offset: 0x00025CBD
		public bool PublishSymbols
		{
			get
			{
				return this.publishSymbols;
			}
			set
			{
				this.publishSymbols = value;
			}
		}

		// Token: 0x04000293 RID: 659
		[DefaultSerialization("libid")]
		[StorageVersion("3.5.3.0")]
		internal string strLibraryId;

		// Token: 0x04000294 RID: 660
		[DefaultSerialization("name")]
		[StorageVersion("3.5.3.0")]
		internal string strNamespace;

		// Token: 0x04000295 RID: 661
		[DefaultSerialization("referencinglib")]
		[StorageVersion("3.5.3.0")]
		internal string strReferencingLibrary;

		// Token: 0x04000296 RID: 662
		[DefaultSerialization("outofpool")]
		[StorageVersion("3.5.3.0")]
		internal bool bOutOfPool;

		// Token: 0x04000297 RID: 663
		[DefaultSerialization("id")]
		[StorageVersion("3.5.3.0")]
		internal int nId;

		// Token: 0x04000298 RID: 664
		[DefaultSerialization("QO")]
		[StorageVersion("3.5.3.50")]
		[StorageDefaultValue(false)]
		[StorageIgnorable]
		internal bool qualifiedOnly;

		// Token: 0x04000299 RID: 665
		[DefaultSerialization("PublishSymbols")]
		[StorageVersion("3.5.16.0")]
		[StorageDefaultValue(false)]
		[StorageIgnorable]
		internal bool publishSymbols;
	}
}
