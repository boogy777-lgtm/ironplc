using System;
using System.Diagnostics;
using System.Linq;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D1 RID: 209
	[TypeGuid("{467AC7F9-D9FC-4E1D-9FB4-9E5DDE0A44C2}")]
	[StorageVersion("3.5.8.0")]
	public class CompileOptionsSerializable : GenericObject2, ICompileOptionsSerializable
	{
		// Token: 0x06000EAB RID: 3755 RVA: 0x00026DD4 File Offset: 0x00025DD4
		public CompileOptionsSerializable()
		{
			this._compilerversion = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse();
			this.UnicodeToSave = APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers;
			this.ReplaceConstantsToSave = APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants;
			this.LoggingInBreakpointsToSave = APEnvironmentFacade.Instance.CompileOptions.EnableBreakpointLogging;
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x00026E58 File Offset: 0x00025E58
		public bool CompileOptionsChanged()
		{
			return this._compilerversion != APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() || this.UnicodeToSave != APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers || this.ReplaceConstantsToSave != APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants || this.LoggingInBreakpointsToSave != APEnvironmentFacade.Instance.CompileOptions.EnableBreakpointLogging;
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000EAD RID: 3757 RVA: 0x00026ECB File Offset: 0x00025ECB
		// (set) Token: 0x06000EAE RID: 3758 RVA: 0x00026ED3 File Offset: 0x00025ED3
		[DefaultSerialization("ReplaceConstants")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		public bool ReplaceConstantsToSave { get; set; } = true;

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000EAF RID: 3759 RVA: 0x00026EDC File Offset: 0x00025EDC
		// (set) Token: 0x06000EB0 RID: 3760 RVA: 0x00026F1C File Offset: 0x00025F1C
		[DefaultSerialization("CompilerVersion")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		public int[] CompilerVersionToSave
		{
			get
			{
				return new int[]
				{
					this._compilerversion.Revision,
					this._compilerversion.Build,
					this._compilerversion.Minor,
					this._compilerversion.Major
				};
			}
			set
			{
				Debug.Assert(value.Count<int>() == 4);
				this._compilerversion = new Version(value[3], value[2], value[1], value[0]);
			}
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000EB1 RID: 3761 RVA: 0x00026F43 File Offset: 0x00025F43
		// (set) Token: 0x06000EB2 RID: 3762 RVA: 0x00026F4B File Offset: 0x00025F4B
		[DefaultSerialization("Unicode")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		public bool UnicodeToSave { get; set; }

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000EB3 RID: 3763 RVA: 0x00026F54 File Offset: 0x00025F54
		// (set) Token: 0x06000EB4 RID: 3764 RVA: 0x00026F5C File Offset: 0x00025F5C
		[DefaultSerialization("LoggingInBreakpoints")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		public bool LoggingInBreakpointsToSave { get; set; } = true;

		// Token: 0x040002A0 RID: 672
		private Version _compilerversion = new Version(0, 0, 0, 0);
	}
}
