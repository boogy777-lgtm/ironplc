using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DE RID: 222
	[TypeGuid("{e5e60e01-536d-4a81-9d91-49c826de4517}")]
	[StorageVersion("3.3.0.0")]
	public class PrecompileLibInfo : GenericObject2, IPrecompileLibInfo
	{
		// Token: 0x06000F91 RID: 3985 RVA: 0x0000AC39 File Offset: 0x00009C39
		public PrecompileLibInfo()
		{
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x0002AB59 File Offset: 0x00029B59
		public PrecompileLibInfo(string stPath, string stNamespace, bool bPublishSymbols, bool bSystemLibrary, bool bQualifiedOnlyLocal)
		{
			this.m_stPath = stPath;
			this.m_stNamespace = stNamespace;
			this.m_bPublishSymbols = bPublishSymbols;
			this.m_bSystemLibrary = bSystemLibrary;
			this.m_bQualifiedOnlyLocal = bQualifiedOnlyLocal;
		}

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06000F93 RID: 3987 RVA: 0x0002AB86 File Offset: 0x00029B86
		public string Identification
		{
			get
			{
				return this.m_stPath;
			}
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x06000F94 RID: 3988 RVA: 0x0002AB8E File Offset: 0x00029B8E
		public string Namespace
		{
			get
			{
				return this.m_stNamespace;
			}
		}

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x06000F95 RID: 3989 RVA: 0x0002AB96 File Offset: 0x00029B96
		public bool PublishSymbols
		{
			get
			{
				return this.m_bPublishSymbols;
			}
		}

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x0002AB9E File Offset: 0x00029B9E
		public bool SystemLibrary
		{
			get
			{
				return this.m_bSystemLibrary;
			}
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x0002ABA6 File Offset: 0x00029BA6
		public bool QualifiedOnly
		{
			get
			{
				return this.m_bQualifiedOnlyLocal;
			}
		}

		// Token: 0x04000395 RID: 917
		[DefaultSerialization("Path")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stPath;

		// Token: 0x04000396 RID: 918
		[DefaultSerialization("Namespace")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stNamespace;

		// Token: 0x04000397 RID: 919
		[DefaultSerialization("PublishSymbols")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bPublishSymbols;

		// Token: 0x04000398 RID: 920
		[DefaultSerialization("SystemLibrary")]
		[StorageVersion("3.3.2.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool m_bSystemLibrary;

		// Token: 0x04000399 RID: 921
		[DefaultSerialization("QualifiedOnlyLocal")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool m_bQualifiedOnlyLocal;
	}
}
