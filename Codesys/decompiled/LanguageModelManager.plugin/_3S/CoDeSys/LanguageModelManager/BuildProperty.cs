using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000158 RID: 344
	[TypeGuid("{24568A24-C491-472c-A21F-EE5D33859FAB}")]
	[StorageVersion("3.3.0.0")]
	public class BuildProperty : GenericObject2, IBuildProperty6, IBuildProperty5, IBuildProperty4, IBuildProperty3, IBuildProperty2, IBuildProperty, IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x17000777 RID: 1911
		// (get) Token: 0x06001BBC RID: 7100 RVA: 0x0004E654 File Offset: 0x0004D654
		public static Guid Guid
		{
			get
			{
				return BuildProperty.My_Guid;
			}
		}

		// Token: 0x17000778 RID: 1912
		// (get) Token: 0x06001BBD RID: 7101 RVA: 0x0004E65B File Offset: 0x0004D65B
		// (set) Token: 0x06001BBE RID: 7102 RVA: 0x0004E663 File Offset: 0x0004D663
		public bool ExcludeFromBuild
		{
			get
			{
				return this._bExcludeFromBuild;
			}
			set
			{
				this._bExcludeFromBuild = value;
			}
		}

		// Token: 0x17000779 RID: 1913
		// (get) Token: 0x06001BBF RID: 7103 RVA: 0x0004E65B File Offset: 0x0004D65B
		// (set) Token: 0x06001BC0 RID: 7104 RVA: 0x0004E663 File Offset: 0x0004D663
		public bool ExcludeFromBuildLocal
		{
			get
			{
				return this._bExcludeFromBuild;
			}
			set
			{
				this._bExcludeFromBuild = value;
			}
		}

		// Token: 0x1700077A RID: 1914
		// (get) Token: 0x06001BC1 RID: 7105 RVA: 0x0004E66C File Offset: 0x0004D66C
		// (set) Token: 0x06001BC2 RID: 7106 RVA: 0x0004E674 File Offset: 0x0004D674
		public bool External
		{
			get
			{
				return this._bExternal;
			}
			set
			{
				this._bExternal = value;
			}
		}

		// Token: 0x1700077B RID: 1915
		// (get) Token: 0x06001BC3 RID: 7107 RVA: 0x0004E67D File Offset: 0x0004D67D
		// (set) Token: 0x06001BC4 RID: 7108 RVA: 0x0004E685 File Offset: 0x0004D685
		public bool EnableSystemCall
		{
			get
			{
				return this._bEnableSystemCall;
			}
			set
			{
				this._bEnableSystemCall = value;
			}
		}

		// Token: 0x1700077C RID: 1916
		// (get) Token: 0x06001BC5 RID: 7109 RVA: 0x0004E68E File Offset: 0x0004D68E
		// (set) Token: 0x06001BC6 RID: 7110 RVA: 0x0004E696 File Offset: 0x0004D696
		public bool LinkAlways
		{
			get
			{
				return this._bLinkAlways;
			}
			set
			{
				this._bLinkAlways = value;
			}
		}

		// Token: 0x1700077D RID: 1917
		// (get) Token: 0x06001BC7 RID: 7111 RVA: 0x0004E69F File Offset: 0x0004D69F
		// (set) Token: 0x06001BC8 RID: 7112 RVA: 0x0004E6A7 File Offset: 0x0004D6A7
		public string CompilerDefines
		{
			get
			{
				return this._stCompilerDefines;
			}
			set
			{
				if (value == null)
				{
					this._stCompilerDefines = string.Empty;
					return;
				}
				this._stCompilerDefines = value;
			}
		}

		// Token: 0x1700077E RID: 1918
		// (get) Token: 0x06001BC9 RID: 7113 RVA: 0x0004E6BF File Offset: 0x0004D6BF
		// (set) Token: 0x06001BCA RID: 7114 RVA: 0x0004E6CC File Offset: 0x0004D6CC
		public IList<string> Undefines
		{
			get
			{
				return Enumerable.ToReadonlyList<string>(this._undefines);
			}
			set
			{
				this._undefines = value.ToArray<string>();
			}
		}

		// Token: 0x1700077F RID: 1919
		// (get) Token: 0x06001BCB RID: 7115 RVA: 0x0004E6DA File Offset: 0x0004D6DA
		// (set) Token: 0x06001BCC RID: 7116 RVA: 0x0004E6E2 File Offset: 0x0004D6E2
		[DefaultSerialization("MemoryReserveForOnlineChange")]
		[StorageVersion("3.5.12.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[StorageDefaultValue(0)]
		[Obfuscation(Feature = "rename")]
		public int MemoryReserveForOnlineChange { get; set; }

		// Token: 0x040005DD RID: 1501
		[Obfuscation(Feature = "rename")]
		public static readonly Guid My_Guid = new Guid("{24568A24-C491-472c-A21F-EE5D33859FAB}");

		// Token: 0x040005DE RID: 1502
		[DefaultSerialization("ExcludeFromBuild")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private bool _bExcludeFromBuild;

		// Token: 0x040005DF RID: 1503
		[DefaultSerialization("External")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private bool _bExternal;

		// Token: 0x040005E0 RID: 1504
		[DefaultSerialization("EnableSystemCall")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private bool _bEnableSystemCall;

		// Token: 0x040005E1 RID: 1505
		[DefaultSerialization("CompilerDefines")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private string _stCompilerDefines = string.Empty;

		// Token: 0x040005E2 RID: 1506
		[DefaultSerialization("LinkAlways")]
		[StorageVersion("3.3.1.0")]
		[StorageIgnorable]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private bool _bLinkAlways;

		// Token: 0x040005E3 RID: 1507
		[DefaultSerialization("Undefines")]
		[StorageVersion("3.5.11.20")]
		[StorageDefaultValueEmptyCollection]
		[DefaultDuplication(DuplicationMethod.Deep)]
		[Obfuscation(Feature = "rename")]
		private string[] _undefines = new string[0];
	}
}
