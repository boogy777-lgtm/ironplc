using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DF RID: 223
	[TypeGuid("{3a0e9879-33d5-4b0b-be71-ca3b35228a92}")]
	[StorageVersion("3.3.0.0")]
	public class LibraryPlaceholder : GenericObject2, _ILibraryPlaceholder, ILibraryPlaceholder3, ILibraryPlaceholder2, ILibraryPlaceholder
	{
		// Token: 0x06000F98 RID: 3992 RVA: 0x0000AC39 File Offset: 0x00009C39
		public LibraryPlaceholder()
		{
		}

		// Token: 0x06000F99 RID: 3993 RVA: 0x0002ABAE File Offset: 0x00029BAE
		public LibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver)
		{
			this.m_stName = stName;
			this.m_stNamespace = stNamespace;
			this.m_stDefaultLibraryId = stDefaultLibraryId;
			this.m_bPublishSymbols = bPublishSymbols;
			this.m_bLinkAllContent = bLinkAllContent;
			this.m_bLinkInSimulation = bLinkInSimulation;
			this.m_guidResolver = guidResolver;
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x0002ABEB File Offset: 0x00029BEB
		public LibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver, Guid libManGuid) : this(stName, stDefaultLibraryId, stNamespace, bPublishSymbols, bLinkAllContent, bLinkInSimulation, guidResolver)
		{
			this.m_guidLibMan = libManGuid;
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06000F9B RID: 3995 RVA: 0x0002AC06 File Offset: 0x00029C06
		public string Name
		{
			get
			{
				return this.m_stName;
			}
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06000F9C RID: 3996 RVA: 0x0002AC0E File Offset: 0x00029C0E
		public string Namespace
		{
			get
			{
				return this.m_stNamespace;
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06000F9D RID: 3997 RVA: 0x0002AC16 File Offset: 0x00029C16
		public string DefaultLibraryId
		{
			get
			{
				return this.m_stDefaultLibraryId;
			}
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06000F9E RID: 3998 RVA: 0x0002AC1E File Offset: 0x00029C1E
		public bool PublishSymbols
		{
			get
			{
				return this.m_bPublishSymbols;
			}
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06000F9F RID: 3999 RVA: 0x0002AC26 File Offset: 0x00029C26
		public bool LinkAllContent
		{
			get
			{
				return this.m_bLinkAllContent;
			}
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06000FA0 RID: 4000 RVA: 0x0002AC2E File Offset: 0x00029C2E
		public bool LinkInSimulation
		{
			get
			{
				return this.m_bLinkInSimulation;
			}
		}

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06000FA1 RID: 4001 RVA: 0x0002AC36 File Offset: 0x00029C36
		public Guid Resolver
		{
			get
			{
				return this.m_guidResolver;
			}
		}

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06000FA2 RID: 4002 RVA: 0x0002AC3E File Offset: 0x00029C3E
		// (set) Token: 0x06000FA3 RID: 4003 RVA: 0x0002AC46 File Offset: 0x00029C46
		public bool QualifiedOnlyLocal
		{
			get
			{
				return this.m_bQualifiedOnly;
			}
			set
			{
				this.m_bQualifiedOnly = value;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06000FA4 RID: 4004 RVA: 0x0002AC4F File Offset: 0x00029C4F
		public Guid LibManGuid
		{
			get
			{
				return this.m_guidLibMan;
			}
		}

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06000FA5 RID: 4005 RVA: 0x0002AC57 File Offset: 0x00029C57
		// (set) Token: 0x06000FA6 RID: 4006 RVA: 0x0002AC5F File Offset: 0x00029C5F
		public bool Optional
		{
			get
			{
				return this.m_bOptional;
			}
			set
			{
				this.m_bOptional = value;
			}
		}

		// Token: 0x0400039A RID: 922
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public string m_stName;

		// Token: 0x0400039B RID: 923
		[DefaultSerialization("Namespace")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public string m_stNamespace;

		// Token: 0x0400039C RID: 924
		[DefaultSerialization("Default")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public string m_stDefaultLibraryId;

		// Token: 0x0400039D RID: 925
		[DefaultSerialization("Publish")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public bool m_bPublishSymbols;

		// Token: 0x0400039E RID: 926
		[DefaultSerialization("LinkAllContent")]
		[StorageVersion("3.3.2.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public bool m_bLinkAllContent;

		// Token: 0x0400039F RID: 927
		[DefaultSerialization("QualifiedOnly")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public bool m_bQualifiedOnly;

		// Token: 0x040003A0 RID: 928
		[DefaultSerialization("LinkInSimulation")]
		[StorageVersion("3.3.2.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public bool m_bLinkInSimulation;

		// Token: 0x040003A1 RID: 929
		[DefaultSerialization("Resolver")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public Guid m_guidResolver;

		// Token: 0x040003A2 RID: 930
		[DefaultSerialization("LibManGuid")]
		[StorageVersion("3.5.5.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public Guid m_guidLibMan;

		// Token: 0x040003A3 RID: 931
		[DefaultSerialization("Optional")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(false)]
		[Obfuscation(Feature = "rename")]
		public bool m_bOptional;
	}
}
